using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StarBlog.Api.Infrastructure.Persistence;

namespace StarBlog.Api.Hosting.HealthChecks;

/// <summary>
/// 就绪探针：能够打开 EF Core 连接即视为就绪。
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck {
    private readonly StarBlogDbContext _dbContext;

    public DatabaseHealthCheck(StarBlogDbContext dbContext) {
        _dbContext = dbContext;
    }

    /// <summary>尝试打开数据库连接。</summary>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) {
        try {
            await _dbContext.Database.OpenConnectionAsync(cancellationToken);
            await _dbContext.Database.CloseConnectionAsync();
            return HealthCheckResult.Healthy("Database is reachable.");
        }
        catch (Exception exception) {
            return HealthCheckResult.Unhealthy("Database is unreachable.", exception);
        }
    }
}
