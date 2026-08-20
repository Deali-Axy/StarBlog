using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace StarBlog.Api.HealthChecks;

/// <summary>
/// 存活探针：只要进程能响应 HTTP，就返回 Healthy。
/// 适合容器/负载均衡用于判断服务是否需要重启（不依赖数据库/缓存等外部资源）。
/// </summary>
public sealed class LiveHealthCheck : IHealthCheck {
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    ) {
        return Task.FromResult(HealthCheckResult.Healthy("Service is running."));
    }
}

