using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Infrastructure.Time;
using StarBlog.Api.Modules.Notifications.Domain;

namespace StarBlog.Api.Modules.Notifications;

/// <summary>
/// 领取、执行并重试 Outbox 消息。每次处理由 Worker 创建独立 scope。
/// </summary>
public sealed class OutboxProcessor {
    private readonly StarBlogDbContext _db;
    private readonly IReadOnlyDictionary<string, IOutboxHandler> _handlers;
    private readonly OutboxOptions _options;
    private readonly IClock _clock;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        StarBlogDbContext db,
        IEnumerable<IOutboxHandler> handlers,
        IOptions<OutboxOptions> options,
        IClock clock,
        ILogger<OutboxProcessor> logger) {
        _db = db;
        _handlers = handlers.ToDictionary(handler => handler.Type, handler => handler, StringComparer.Ordinal);
        _options = options.Value;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>领取一批到期消息并执行。返回成功处理条数。</summary>
    public async Task<int> ProcessOnceAsync(string workerId, CancellationToken cancellationToken) {
        var now = NotificationOutbox.Truncate(_clock.Now);
        var candidates = await _db.OutboxMessages
            .Where(message =>
                (message.Status == OutboxStatus.Pending || message.Status == OutboxStatus.Processing)
                && message.NextAttemptAt <= now
                && (message.LockedUntil == null || message.LockedUntil < now))
            .OrderBy(message => message.Id)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0) return 0;

        var leaseUntil = now.Add(_options.LeaseDuration);
        var claimed = new List<OutboxMessage>();
        foreach (var candidate in candidates) {
            cancellationToken.ThrowIfCancellationRequested();
            var affected = await _db.OutboxMessages
                .Where(message =>
                    message.Id == candidate.Id
                    && (message.Status == OutboxStatus.Pending || message.Status == OutboxStatus.Processing)
                    && message.NextAttemptAt <= now
                    && (message.LockedUntil == null || message.LockedUntil < now))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(message => message.Status, OutboxStatus.Processing)
                    .SetProperty(message => message.LockedBy, workerId)
                    .SetProperty(message => message.LockedUntil, leaseUntil)
                    .SetProperty(message => message.UpdatedAt, now), cancellationToken);
            if (affected == 1) claimed.Add(candidate);
        }

        var processed = 0;
        foreach (var message in claimed.OrderBy(item => item.Id)) {
            cancellationToken.ThrowIfCancellationRequested();
            if (await ProcessMessageAsync(workerId, message, cancellationToken)) processed++;
        }

        return processed;
    }

    private async Task<bool> ProcessMessageAsync(string workerId, OutboxMessage message, CancellationToken cancellationToken) {
        if (!_handlers.TryGetValue(message.Type, out var handler)) {
            await MarkAsync(workerId, message.Id, OutboxStatus.Dead, error: $"未找到任务处理器：{message.Type}", cancellationToken: cancellationToken);
            return false;
        }

        try {
            await handler.HandleAsync(message, cancellationToken);
            await MarkAsync(workerId, message.Id, OutboxStatus.Succeeded, cancellationToken: cancellationToken);
            _logger.LogInformation("Outbox 执行成功：{Type} #{Id}", message.Type, message.Id);
            return true;
        }
        catch (Exception exception) {
            await ScheduleRetryOrDeadAsync(workerId, message, exception, cancellationToken);
            return false;
        }
    }

    private async Task MarkAsync(
        string workerId,
        long id,
        OutboxStatus status,
        int? attempt = null,
        DateTime? nextAttemptAt = null,
        string? error = null,
        CancellationToken cancellationToken = default) {
        var now = NotificationOutbox.Truncate(_clock.Now);
        await _db.OutboxMessages
            .Where(message => message.Id == id && message.LockedBy == workerId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(message => message.Status, status)
                .SetProperty(message => message.LockedBy, (string?)null)
                .SetProperty(message => message.LockedUntil, (DateTime?)null)
                .SetProperty(message => message.UpdatedAt, now)
                .SetProperty(message => message.Attempt, message => attempt ?? message.Attempt)
                .SetProperty(message => message.NextAttemptAt, message => nextAttemptAt ?? message.NextAttemptAt)
                .SetProperty(message => message.LastError, error), cancellationToken);
    }

    private async Task ScheduleRetryOrDeadAsync(string workerId, OutboxMessage message, Exception exception, CancellationToken cancellationToken) {
        var nextAttempt = message.Attempt + 1;
        var error = TrimError(exception.ToString());
        if (nextAttempt >= message.MaxAttempts) {
            _logger.LogError(exception, "Outbox 执行失败（放弃）：{Type} #{Id}", message.Type, message.Id);
            await MarkAsync(workerId, message.Id, OutboxStatus.Dead, nextAttempt, error: error, cancellationToken: cancellationToken);
            return;
        }

        var delay = GetBackoffDelay(nextAttempt);
        var nextAt = NotificationOutbox.Truncate(_clock.Now.Add(delay));
        _logger.LogWarning(exception, "Outbox 将在 {Delay} 后重试：{Type} #{Id}", delay, message.Type, message.Id);
        await MarkAsync(workerId, message.Id, OutboxStatus.Pending, nextAttempt, nextAt, error, cancellationToken);
    }

    private TimeSpan GetBackoffDelay(int attempt) {
        var baseSeconds = Math.Min(Math.Pow(2, attempt), _options.MaxBackoff.TotalSeconds);
        var jitterMs = Random.Shared.Next(0, (int)Math.Max(0, _options.BackoffJitter.TotalMilliseconds));
        return TimeSpan.FromSeconds(baseSeconds) + TimeSpan.FromMilliseconds(jitterMs);
    }

    private static string TrimError(string error) => error.Length <= 4000 ? error : error[..4000];
}
