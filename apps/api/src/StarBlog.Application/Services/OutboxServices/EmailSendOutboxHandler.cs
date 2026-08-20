using System.Text.Json;
using StarBlog.Data.Models;
using StarBlog.Application.Services;

namespace StarBlog.Application.Services.OutboxServices;

/// <summary>
/// 邮件发送任务处理器
/// </summary>
public class EmailSendOutboxHandler : IOutboxHandler {
    private readonly EmailService _emailService;

    public EmailSendOutboxHandler(EmailService emailService) {
        _emailService = emailService;
    }

    public string Type => OutboxTaskTypes.EmailSend;

    public async Task HandleAsync(OutboxMessage message, CancellationToken cancellationToken) {
        var payload = JsonSerializer.Deserialize<OutboxEmailPayload>(message.Payload);
        if (payload == null) throw new InvalidOperationException($"Outbox payload 反序列化失败：{message.Id}");

        await _emailService.SendEmailAsync(payload.Subject, payload.HtmlBody, payload.ToName, payload.ToAddress);
    }
}
