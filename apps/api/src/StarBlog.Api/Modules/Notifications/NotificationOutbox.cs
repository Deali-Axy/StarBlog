using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Infrastructure.Time;
using StarBlog.Api.Modules.Notifications.Domain;

namespace StarBlog.Api.Modules.Notifications;

/// <summary>
/// 把通知任务写入 Outbox。已存在相同 DedupKey 的消息会被跳过。
/// </summary>
public sealed class NotificationOutbox : INotificationOutbox {
    private readonly StarBlogDbContext _db;
    private readonly IClock _clock;
    private readonly OutboxOptions _options;
    private readonly ILogger<NotificationOutbox> _logger;

    public NotificationOutbox(
        StarBlogDbContext db,
        IClock clock,
        IOptions<OutboxOptions> options,
        ILogger<NotificationOutbox> logger) {
        _db = db;
        _clock = clock;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>入队邮件发送任务。重复 DedupKey 会被忽略。</summary>
    public async Task EnqueueEmailAsync(
        string subject,
        string htmlBody,
        string toName,
        string toAddress,
        string? dedupKey = null,
        CancellationToken cancellationToken = default) {
        if (!string.IsNullOrWhiteSpace(dedupKey)) {
            var exists = await _db.OutboxMessages.AnyAsync(message => message.DedupKey == dedupKey, cancellationToken);
            if (exists) {
                _logger.LogInformation("Outbox 跳过重复任务：{DedupKey}", dedupKey);
                return;
            }
        }

        var now = Truncate(_clock.Now);
        _db.OutboxMessages.Add(new OutboxMessage {
            Type = OutboxTaskTypes.EmailSend,
            DedupKey = dedupKey,
            Payload = JsonSerializer.Serialize(new OutboxEmailPayload {
                Subject = subject,
                HtmlBody = htmlBody,
                ToName = toName,
                ToAddress = toAddress
            }),
            Status = OutboxStatus.Pending,
            MaxAttempts = _options.DefaultMaxAttempts,
            NextAttemptAt = now,
            CreatedAt = now,
            UpdatedAt = now
        });
        _logger.LogInformation("Outbox 入队：{Type}，DedupKey：{DedupKey}", OutboxTaskTypes.EmailSend, dedupKey);
    }

    internal static DateTime Truncate(DateTime value) =>
        new(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Kind);
}
