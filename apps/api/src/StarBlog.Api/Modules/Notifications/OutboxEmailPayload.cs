namespace StarBlog.Api.Modules.Notifications;

/// <summary>邮件发送任务的 JSON 载荷。</summary>
public sealed record OutboxEmailPayload {
    public required string Subject { get; init; }
    public required string HtmlBody { get; init; }
    public required string ToName { get; init; }
    public required string ToAddress { get; init; }
}

/// <summary>已知的 Outbox 任务类型。</summary>
public static class OutboxTaskTypes {
    public const string EmailSend = "email.send";
}
