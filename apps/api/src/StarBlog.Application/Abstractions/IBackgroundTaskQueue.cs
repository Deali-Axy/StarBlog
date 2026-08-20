namespace StarBlog.Application.Abstractions;

public interface IBackgroundTaskQueue {
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem);

    ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}
