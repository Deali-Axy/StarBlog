using StarBlog.Application.Services.VisitRecordServices;

namespace StarBlog.Api.Services.VisitRecordServices;

public class VisitRecordWorker : BackgroundService {
    private readonly VisitRecordQueueService _logQueue;
    private readonly TimeSpan _executeInterval = TimeSpan.FromSeconds(30);

    public VisitRecordWorker(VisitRecordQueueService logQueue) {
        _logQueue = logQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            await _logQueue.WriteLogsToDatabaseAsync(stoppingToken);
            await Task.Delay(_executeInterval, stoppingToken);
        }
    }
}
