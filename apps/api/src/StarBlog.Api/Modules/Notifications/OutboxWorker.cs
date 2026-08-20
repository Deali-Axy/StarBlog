using Microsoft.Extensions.Options;

namespace StarBlog.Api.Modules.Notifications;

/// <summary>
/// Outbox HostedService。每次循环通过 IServiceScopeFactory 创建独立 scope，不持有 Scoped DbContext。
/// </summary>
public sealed class OutboxWorker : BackgroundService {
    private readonly ILogger<OutboxWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OutboxOptions _options;
    private readonly string _workerId = $"{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";

    public OutboxWorker(ILogger<OutboxWorker> logger, IServiceScopeFactory scopeFactory, IOptions<OutboxOptions> options) {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _logger.LogInformation("OutboxWorker started: {WorkerId}", _workerId);

        while (!stoppingToken.IsCancellationRequested) {
            try {
                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
                var processed = await processor.ProcessOnceAsync(_workerId, stoppingToken);
                if (processed == 0) {
                    await Task.Delay(_options.PollInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException) {
                break;
            }
            catch (Exception exception) {
                _logger.LogError(exception, "OutboxWorker loop error");
                try {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
                catch (OperationCanceledException) {
                    break;
                }
            }
        }

        _logger.LogInformation("OutboxWorker stopped: {WorkerId}", _workerId);
    }
}
