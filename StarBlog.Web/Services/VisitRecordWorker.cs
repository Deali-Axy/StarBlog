namespace StarBlog.Web.Services;

public class VisitRecordWorker : BackgroundService {
    private readonly ILogger<VisitRecordWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly VisitRecordQueueService _logQueue;
    private readonly TimeSpan _executeInterval = TimeSpan.FromSeconds(30);

    public VisitRecordWorker(ILogger<VisitRecordWorker> logger, IServiceScopeFactory scopeFactory, VisitRecordQueueService logQueue) {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _logQueue = logQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            await _logQueue.WriteLogsToDatabaseAsync(stoppingToken);
            await Task.Delay(_executeInterval, stoppingToken);
            _logger.LogDebug("后台任务 VisitRecordWorker ExecuteAsync");
        }
    }
}