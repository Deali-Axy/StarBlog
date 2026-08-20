using FreeSql;
using Microsoft.Extensions.Options;
using StarBlog.Data.Models;

namespace StarBlog.Application.Services.OutboxServices;

/// <summary>
/// Outbox 处理器（领取 + 执行 + 重试）
/// </summary>
public class OutboxProcessor {
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly IFreeSql _fsql;
    private readonly IBaseRepository<OutboxMessage> _outboxRepo;
    private readonly IReadOnlyDictionary<string, IOutboxHandler> _handlersByType;
    private readonly OutboxOptions _options;

    public OutboxProcessor(
        ILogger<OutboxProcessor> logger,
        IFreeSql fsql,
        IBaseRepository<OutboxMessage> outboxRepo,
        IEnumerable<IOutboxHandler> handlers,
        IOptions<OutboxOptions> options
    ) {
        _logger = logger;
        _fsql = fsql;
        _outboxRepo = outboxRepo;
        _handlersByType = handlers.ToDictionary(a => a.Type, a => a, StringComparer.Ordinal);
        _options = options.Value;
    }

    public async Task<int> ProcessOnceAsync(string workerId, CancellationToken cancellationToken) {
        var now = NormalizeTime(DateTime.Now);
        var candidateList = await _outboxRepo.Select
            .Where(a =>
                (a.Status == OutboxStatus.Pending || a.Status == OutboxStatus.Processing)
                && a.NextAttemptAt <= now
                && (a.LockedUntil == null || a.LockedUntil < now))
            .OrderBy(a => a.Id)
            .Limit(_options.BatchSize)
            .ToListAsync();

        if (candidateList.Count == 0) return 0;

        var leaseUntil = now.Add(_options.LeaseDuration);
        var claimedIds = new List<long>(candidateList.Count);

        foreach (var candidate in candidateList) {
            cancellationToken.ThrowIfCancellationRequested();

            var affected = await _fsql.Update<OutboxMessage>()
                .Set(a => a.Status, OutboxStatus.Processing)
                .Set(a => a.LockedBy, workerId)
                .Set(a => a.LockedUntil, leaseUntil)
                .Set(a => a.UpdatedAt, now)
                .Where(a =>
                    a.Id == candidate.Id
                    && (a.Status == OutboxStatus.Pending || a.Status == OutboxStatus.Processing)
                    && a.NextAttemptAt <= now
                    && (a.LockedUntil == null || a.LockedUntil < now))
                .ExecuteAffrowsAsync();

            if (affected == 1) {
                claimedIds.Add(candidate.Id);
            }
        }

        if (claimedIds.Count == 0) return 0;

        var messages = await _outboxRepo.Select
            .Where(a => claimedIds.Contains(a.Id))
            .ToListAsync();

        var processed = 0;
        foreach (var message in messages.OrderBy(a => a.Id)) {
            cancellationToken.ThrowIfCancellationRequested();
            processed += await ProcessMessageAsync(workerId, message, cancellationToken) ? 1 : 0;
        }

        return processed;
    }

    private async Task<bool> ProcessMessageAsync(string workerId, OutboxMessage message, CancellationToken cancellationToken) {
        if (!_handlersByType.TryGetValue(message.Type, out var handler)) {
            await MarkDeadAsync(workerId, message.Id, $"未找到任务处理器：{message.Type}");
            return false;
        }

        try {
            await handler.HandleAsync(message, cancellationToken);
            await MarkSucceededAsync(workerId, message.Id);
            _logger.LogInformation("Outbox 执行成功：{Type} #{Id}", message.Type, message.Id);
            return true;
        }
        catch (Exception ex) {
            await ScheduleRetryOrDeadAsync(workerId, message, ex);
            return false;
        }
    }

    private async Task MarkSucceededAsync(string workerId, long id) {
        var now = NormalizeTime(DateTime.Now);
        await _fsql.Update<OutboxMessage>()
            .Set(a => a.Status, OutboxStatus.Succeeded)
            .Set(a => a.LockedBy, null)
            .Set(a => a.LockedUntil, null)
            .Set(a => a.UpdatedAt, now)
            .Where(a => a.Id == id && a.LockedBy == workerId)
            .ExecuteAffrowsAsync();
    }

    private async Task MarkDeadAsync(string workerId, long id, string error) {
        var now = NormalizeTime(DateTime.Now);
        await _fsql.Update<OutboxMessage>()
            .Set(a => a.Status, OutboxStatus.Dead)
            .Set(a => a.LastError, TrimError(error))
            .Set(a => a.LockedBy, null)
            .Set(a => a.LockedUntil, null)
            .Set(a => a.UpdatedAt, now)
            .Where(a => a.Id == id && a.LockedBy == workerId)
            .ExecuteAffrowsAsync();
    }

    private async Task ScheduleRetryOrDeadAsync(string workerId, OutboxMessage message, Exception exception) {
        var now = NormalizeTime(DateTime.Now);
        var nextAttempt = message.Attempt + 1;
        var error = TrimError(exception.ToString());

        if (nextAttempt >= message.MaxAttempts) {
            _logger.LogError(exception, "Outbox 执行失败（放弃）：{Type} #{Id}，已尝试：{Attempt}", message.Type, message.Id, nextAttempt);
            await _fsql.Update<OutboxMessage>()
                .Set(a => a.Status, OutboxStatus.Dead)
                .Set(a => a.Attempt, nextAttempt)
                .Set(a => a.LastError, error)
                .Set(a => a.LockedBy, null)
                .Set(a => a.LockedUntil, null)
                .Set(a => a.UpdatedAt, now)
                .Where(a => a.Id == message.Id && a.LockedBy == workerId)
                .ExecuteAffrowsAsync();
            return;
        }

        var delay = GetBackoffDelay(nextAttempt);
        var nextAt = NormalizeTime(now.Add(delay));
        _logger.LogWarning(exception, "Outbox 执行失败，将在 {Delay} 后重试：{Type} #{Id}，下一次尝试：{Attempt}",
            delay, message.Type, message.Id, nextAttempt + 1);

        await _fsql.Update<OutboxMessage>()
            .Set(a => a.Status, OutboxStatus.Pending)
            .Set(a => a.Attempt, nextAttempt)
            .Set(a => a.NextAttemptAt, nextAt)
            .Set(a => a.LastError, error)
            .Set(a => a.LockedBy, null)
            .Set(a => a.LockedUntil, null)
            .Set(a => a.UpdatedAt, now)
            .Where(a => a.Id == message.Id && a.LockedBy == workerId)
            .ExecuteAffrowsAsync();
    }

    private TimeSpan GetBackoffDelay(int attempt) {
        var baseSeconds = Math.Min(Math.Pow(2, attempt), _options.MaxBackoff.TotalSeconds);
        var jitterMs = Random.Shared.Next(0, (int)Math.Max(0, _options.BackoffJitter.TotalMilliseconds));
        return TimeSpan.FromSeconds(baseSeconds) + TimeSpan.FromMilliseconds(jitterMs);
    }

    private static string TrimError(string error) {
        if (string.IsNullOrWhiteSpace(error)) return error;
        return error.Length <= 4000 ? error : error[..4000];
    }

    private static DateTime NormalizeTime(DateTime value) {
        return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Kind);
    }
}
