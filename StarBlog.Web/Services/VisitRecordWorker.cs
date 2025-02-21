namespace StarBlog.Web.Services;

public class VisitRecordWorker : BackgroundService {
    private readonly ILogger<VisitRecordWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public VisitRecordWorker(ILogger<VisitRecordWorker> logger, IServiceScopeFactory scopeFactory) {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            using (var scope = _scopeFactory.CreateScope()) {
                var logQueue = scope.ServiceProvider.GetRequiredService<VisitRecordQueueService>();
                await logQueue.WriteLogsToDatabaseAsync(stoppingToken);
            }

            // 暂停一会再继续检查
            await Task.Delay(5000, stoppingToken); // 每5秒写一次日志
            
            _logger.LogDebug("后台任务 VisitRecordWorker ExecuteAsync");
        }
    }
}