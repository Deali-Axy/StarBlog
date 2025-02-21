namespace StarBlog.Web.Services;

public class VisitRecordWorker : BackgroundService {
    private readonly ILogger<VisitRecordWorker> _logger;
    private readonly VisitRecordQueueService _logQueue;

    public VisitRecordWorker(ILogger<VisitRecordWorker> logger, VisitRecordQueueService logQueue) {
        _logger = logger;
        _logQueue = logQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        await _logQueue.WriteLogsToDatabaseAsync(stoppingToken);
    }
}