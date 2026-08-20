namespace StarBlog.Api.Modules.Notifications;

/// <summary>
/// 业务模块入队通知任务的公开契约。调用方必须在同一 DbContext 上随后 SaveChanges。
/// </summary>
public interface INotificationOutbox {
    Task EnqueueEmailAsync(
        string subject,
        string htmlBody,
        string toName,
        string toAddress,
        string? dedupKey = null,
        CancellationToken cancellationToken = default);
}
