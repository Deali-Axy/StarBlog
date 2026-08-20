using System.Threading.Channels;

namespace StarBlog.Api.Infrastructure.Background;

/// <summary>
/// 基于 Channel 的无界后台任务队列。
/// </summary>
public sealed class BackgroundTaskQueue : IBackgroundTaskQueue {
    private readonly Channel<Func<CancellationToken, Task>> _queue = Channel.CreateUnbounded<Func<CancellationToken, Task>>(
        new UnboundedChannelOptions {
            SingleReader = true,
            SingleWriter = false
        });

    /// <summary>将工作项写入队列。</summary>
    public ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem) {
        ArgumentNullException.ThrowIfNull(workItem);
        return _queue.Writer.WriteAsync(workItem);
    }

    /// <summary>取出下一个工作项；取消时抛出 <see cref="OperationCanceledException"/>。</summary>
    public ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken) {
        return _queue.Reader.ReadAsync(cancellationToken);
    }
}
