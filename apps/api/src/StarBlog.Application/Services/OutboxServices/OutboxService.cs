using System.Text.Json;
using FreeSql;
using Microsoft.Extensions.Options;
using StarBlog.Data.Models;

namespace StarBlog.Application.Services.OutboxServices;

/// <summary>
/// Outbox 入队服务
/// <para>负责把“需要异步处理的任务”写入 app.db 的 outbox_message 表。</para>
/// </summary>
public class OutboxService {
    private readonly ILogger<OutboxService> _logger;
    private readonly IBaseRepository<OutboxMessage> _outboxRepo;
    private readonly OutboxOptions _options;

    public OutboxService(ILogger<OutboxService> logger, IBaseRepository<OutboxMessage> outboxRepo, IOptions<OutboxOptions> options) {
        _logger = logger;
        _outboxRepo = outboxRepo;
        _options = options.Value;
    }

    /// <summary>
    /// 入队一条通用 Outbox 任务
    /// <para>payloadJson 建议为 JSON 字符串，便于后续扩展与排查。</para>
    /// </summary>
    public async Task<long> EnqueueAsync(
        string type,
        string payloadJson,
        string? dedupKey = null,
        int? maxAttempts = null,
        DateTime? nextAttemptAt = null
    ) {
        var now = NormalizeTime(DateTime.Now);
        var entity = new OutboxMessage {
            Type = type,
            DedupKey = dedupKey,
            Payload = payloadJson,
            Status = OutboxStatus.Pending,
            Attempt = 0,
            MaxAttempts = maxAttempts ?? _options.DefaultMaxAttempts,
            NextAttemptAt = NormalizeTime(nextAttemptAt ?? now),
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _outboxRepo.InsertAsync(entity);
        _logger.LogInformation("Outbox 入队：{Type} #{Id}，DedupKey：{DedupKey}", entity.Type, entity.Id, entity.DedupKey);
        return entity.Id;
    }

    /// <summary>
    /// 入队一条“发送邮件”任务
    /// <para>注意：该方法只负责入库，不负责立即发送。</para>
    /// </summary>
    public async Task<long> EnqueueEmailAsync(
        string subject,
        string htmlBody,
        string toName,
        string toAddress,
        string? dedupKey = null,
        int? maxAttempts = null,
        DateTime? nextAttemptAt = null
    ) {
        var payload = new OutboxEmailPayload {
            Subject = subject,
            HtmlBody = htmlBody,
            ToName = toName,
            ToAddress = toAddress,
        };

        return await EnqueueAsync(
            OutboxTaskTypes.EmailSend,
            JsonSerializer.Serialize(payload),
            dedupKey,
            maxAttempts,
            nextAttemptAt
        );
    }

    private static DateTime NormalizeTime(DateTime value) {
        return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Kind);
    }
}
