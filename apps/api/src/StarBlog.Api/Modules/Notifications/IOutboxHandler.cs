using StarBlog.Api.Modules.Notifications.Domain;

namespace StarBlog.Api.Modules.Notifications;

/// <summary>
/// Outbox 任务处理器。每种 Type 对应一个实现。
/// </summary>
public interface IOutboxHandler {
    string Type { get; }
    Task HandleAsync(OutboxMessage message, CancellationToken cancellationToken);
}
