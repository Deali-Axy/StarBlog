namespace StarBlog.Api.Modules.Notifications.Domain;

/// <summary>Outbox 消息状态。</summary>
public enum OutboxStatus {
    Pending = 0,
    Processing = 1,
    Succeeded = 2,
    Dead = 3
}

/// <summary>
/// 可靠异步任务。与触发它的业务写入共享同一个 EF Core 事务。
/// </summary>
public sealed class OutboxMessage {
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? DedupKey { get; set; }
    public string Payload { get; set; } = string.Empty;
    public OutboxStatus Status { get; set; } = OutboxStatus.Pending;
    public int Attempt { get; set; }
    public int MaxAttempts { get; set; } = 5;
    public DateTime NextAttemptAt { get; set; }
    public DateTime? LockedUntil { get; set; }
    public string? LockedBy { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
