using StarBlog.Application.Abstractions;

namespace StarBlog.Web.Services.BackgroundTasks;

public sealed class BackgroundTaskWorker : BackgroundService {
    private readonly ILogger<BackgroundTaskWorker> _logger;
    private readonly IBackgroundTaskQueue _queue;

    public BackgroundTaskWorker(ILogger<BackgroundTaskWorker> logger, IBackgroundTaskQueue queue) {
        _logger = logger;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _logger.LogInformation("BackgroundTaskWorker started");

        while (!stoppingToken.IsCancellationRequested) {
            try {
                var workItem = await _queue.DequeueAsync(stoppingToken);
                await workItem(stoppingToken);
            }
            catch (OperationCanceledException) {
                break;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "BackgroundTaskWorker loop error");
                try {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
                catch (OperationCanceledException) {
                    break;
                }
            }
        }

        _logger.LogInformation("BackgroundTaskWorker stopped");
    }
}
