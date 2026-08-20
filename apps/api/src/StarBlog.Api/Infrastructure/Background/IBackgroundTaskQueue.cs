namespace StarBlog.Api.Infrastructure.Background;

/// <summary>
/// 进程内有界任务队列。用于图片本地化等不需要持久化的后台工作。
/// </summary>
public interface IBackgroundTaskQueue {
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem);
    ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}
