using System.Threading.Channels;
using StarBlog.Application.Abstractions;

namespace StarBlog.Application.Services;

public sealed class BackgroundTaskQueue : IBackgroundTaskQueue {
    private readonly Channel<Func<CancellationToken, Task>> _queue;

    public BackgroundTaskQueue() {
        _queue = Channel.CreateUnbounded<Func<CancellationToken, Task>>(new UnboundedChannelOptions {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem) {
        if (workItem == null) throw new ArgumentNullException(nameof(workItem));
        return _queue.Writer.WriteAsync(workItem);
    }

    public ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken) {
        return _queue.Reader.ReadAsync(cancellationToken);
    }
}
