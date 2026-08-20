using FreeSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace StarBlog.Api.HealthChecks;

/// <summary>
/// Readiness：检查 FreeSql 业务库是否可用。
/// 这里使用最轻量的探测语句（select 1），用于验证连接与执行通道正常。
/// </summary>
public sealed class FreeSqlHealthCheck : IHealthCheck {
    private readonly IFreeSql _freeSql;

    public FreeSqlHealthCheck(IFreeSql freeSql) {
        _freeSql = freeSql;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    ) {
        try {
            _freeSql.Ado.ExecuteScalar("select 1");
            return Task.FromResult(HealthCheckResult.Healthy("FreeSql database is reachable."));
        }
        catch (Exception ex) {
            return Task.FromResult(HealthCheckResult.Unhealthy("FreeSql database check failed.", ex));
        }
    }
}

