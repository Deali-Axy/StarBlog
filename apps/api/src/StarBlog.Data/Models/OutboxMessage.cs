using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

public enum OutboxStatus {
    Pending = 0,
    Processing = 1,
    Succeeded = 2,
    Dead = 3,
}

/// <summary>
/// Outbox 消息（可靠异步任务）
/// <para>存放于主库 app.db，由 FreeSQL CodeFirst 自动建表/同步结构。</para>
/// </summary>
[Table(Name = "outbox_message")]
public class OutboxMessage {
    /// <summary>
    /// 自增主键（用于顺序分页拉取/排队） 
    /// </summary>
    [Column(IsIdentity = true, IsPrimary = true)]
    public long Id { get; set; }

    /// <summary>
    /// 任务类型
    /// <para>示例：email.send</para>
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// 幂等键（可选）
    /// <para>用于避免同一业务事件重复入队（例如：reply:{replyId}）。</para>
    /// </summary>
    public string? DedupKey { get; set; }

    /// <summary>
    /// 任务载荷（JSON 字符串） 
    /// </summary>
    [Column(StringLength = -2)]
    public string Payload { get; set; } = null!;

    /// <summary>
    /// 当前状态
    /// </summary>
    public OutboxStatus Status { get; set; } = OutboxStatus.Pending;

    /// <summary>
    /// 已尝试次数
    /// </summary>
    public int Attempt { get; set; } = 0;

    /// <summary>
    /// 最大尝试次数
    /// <para>超过该次数将标记为 Dead。</para>
    /// </summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>
    /// 下次可执行时间（用于退避重试）
    /// </summary>
    public DateTime NextAttemptAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 租约锁：锁过期时间（多实例/并发 Worker 下避免重复处理）
    /// </summary>
    public DateTime? LockedUntil { get; set; }

    /// <summary>
    /// 租约锁：持有者标识（可用于排查与追踪）
    /// </summary>
    public string? LockedBy { get; set; }

    /// <summary>
    /// 最后一次失败原因（可截断）
    /// </summary>
    [Column(StringLength = 4000)]
    public string? LastError { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
