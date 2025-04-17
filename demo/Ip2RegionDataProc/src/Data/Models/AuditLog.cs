namespace Ip2RegionDataProc.Data.Models;

public class AuditLog {
    public long Id { get; set; }

    /// <summary>
    /// 事件唯一标识
    /// </summary>
    public Guid EventId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 事件类型（例如：登录、登出、数据修改等）
    /// </summary>
    public string EventType { get; set; }

    /// <summary>
    /// 执行操作的用户名
    /// </summary>
    public string Username { get; set; } = Environment.UserName;

    /// <summary>
    /// 事件发生的时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 被操作的实体名称
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// 被操作的实体标识
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// 修改前的数据，可根据实际情况以JSON格式存储
    /// </summary>
    public string? OriginalValues { get; set; }

    /// <summary>
    /// 修改后的数据，可根据实际情况以JSON格式存储
    /// </summary>
    public string? CurrentValues { get; set; }

    /// <summary>
    /// 具体的更改内容，可根据实际情况以JSON格式存储
    /// </summary>
    public string? Changes { get; set; }

    /// <summary>
    /// 事件描述
    /// </summary>
    public string? Description { get; set; }
}