using System.Text.Json;
using StarBlog.Api.Infrastructure.Email;
using StarBlog.Api.Modules.Notifications.Domain;

namespace StarBlog.Api.Modules.Notifications;

/// <summary>
/// 处理 email.send 任务。
/// </summary>
public sealed class EmailSendOutboxHandler : IOutboxHandler {
    private readonly IEmailSender _emailSender;

    public EmailSendOutboxHandler(IEmailSender emailSender) {
        _emailSender = emailSender;
    }

    public string Type => OutboxTaskTypes.EmailSend;

    /// <summary>反序列化载荷并调用邮件适配器。</summary>
    public async Task HandleAsync(OutboxMessage message, CancellationToken cancellationToken) {
        var payload = JsonSerializer.Deserialize<OutboxEmailPayload>(message.Payload)
                      ?? throw new InvalidOperationException($"Outbox payload 反序列化失败：{message.Id}");
        await _emailSender.SendAsync(payload.Subject, payload.HtmlBody, payload.ToName, payload.ToAddress, cancellationToken);
    }
}
