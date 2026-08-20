using Microsoft.Extensions.Diagnostics.HealthChecks;
using StarBlog.Data;

namespace StarBlog.Api.HealthChecks;

/// <summary>
/// Readiness：检查 EF Core 的 SQLite-Log（访问统计库）是否可连接。
/// </summary>
public sealed class SqliteLogHealthCheck : IHealthCheck {
    private readonly AppDbContext _dbContext;

    public SqliteLogHealthCheck(AppDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    ) {
        try {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("SQLite-Log database is reachable.")
                : HealthCheckResult.Degraded("SQLite-Log database is not reachable (degraded).");
        }
        catch (Exception ex) {
            return HealthCheckResult.Unhealthy("SQLite-Log database check failed.", ex);
        }
    }
}
