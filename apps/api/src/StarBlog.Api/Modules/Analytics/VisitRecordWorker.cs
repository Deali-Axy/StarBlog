namespace StarBlog.Api.Modules.Analytics;

/// <summary>
/// 定时刷新访问记录队列。
/// </summary>
public sealed class VisitRecordWorker : BackgroundService {
    private readonly VisitRecordQueue _queue;

    public VisitRecordWorker(VisitRecordQueue queue) {
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            await _queue.FlushAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
