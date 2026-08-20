using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace StarBlog.Api.Hosting.HealthChecks;

/// <summary>
/// 存活探针：进程能响应 HTTP 即视为健康。
/// </summary>
public sealed class LiveHealthCheck : IHealthCheck {
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) {
        return Task.FromResult(HealthCheckResult.Healthy("Service is running."));
    }
}
