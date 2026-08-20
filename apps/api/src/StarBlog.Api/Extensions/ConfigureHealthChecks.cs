using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StarBlog.Api.HealthChecks;

namespace StarBlog.Api.Extensions;

public static class ConfigureHealthChecks {
    /// <summary>
    /// 为 StarBlog.Api 注册健康检查（Health Checks）。
    /// - Liveness：仅用于判断进程是否还“活着”（不依赖外部资源）
    /// - Readiness：用于判断服务是否“就绪”（会检查关键依赖是否可用，如数据库）
    /// </summary>
    public static void AddStarBlogHealthChecks(this IServiceCollection services) {
        services
            .AddHealthChecks()
            .AddCheck<LiveHealthCheck>(
                name: "live",
                tags: new[] { HealthCheckTags.Live }
            )
            .AddCheck<SqliteLogHealthCheck>(
                name: "sqlite-log",
                tags: new[] { HealthCheckTags.Ready }
            )
            .AddCheck<FreeSqlHealthCheck>(
                name: "freesql",
                tags: new[] { HealthCheckTags.Ready }
            );
    }

    /// <summary>
    /// 映射健康检查端点：
    /// - /health/live：存活探针
    /// - /health/ready：就绪探针
    /// - /health、/healthz：汇总探针（兼容别名）
    /// </summary>
    public static WebApplication MapStarBlogHealthChecks(this WebApplication app) {
        // HealthChecks：最常见的三种探针（以及 /healthz 兼容别名）
        app.MapHealthChecks(
                "/health/live",
                CreateHealthCheckOptions(r => r.Tags.Contains(HealthCheckTags.Live))
            )
            .AllowAnonymous();

        app.MapHealthChecks(
                "/health/ready",
                CreateHealthCheckOptions(r => r.Tags.Contains(HealthCheckTags.Ready))
            )
            .AllowAnonymous();

        app.MapHealthChecks(
                "/health",
                CreateHealthCheckOptions(_ => true)
            )
            .AllowAnonymous();

        app.MapHealthChecks(
                "/healthz",
                CreateHealthCheckOptions(_ => true)
            )
            .AllowAnonymous();

        return app;
    }

    /// <summary>
    /// 统一的 HealthCheckOptions，输出 JSON，并按健康状态返回 HTTP 状态码。
    /// </summary>
    public static HealthCheckOptions CreateHealthCheckOptions(Func<HealthCheckRegistration, bool> predicate) {
        return new HealthCheckOptions {
            Predicate = predicate,
            AllowCachingResponses = false,
            ResultStatusCodes = {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            },
            ResponseWriter = WriteResponseAsync
        };
    }

    private static Task WriteResponseAsync(HttpContext context, HealthReport report) {
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = new {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                error = entry.Value.Exception?.Message,
                data = entry.Value.Data.Count == 0 ? null : entry.Value.Data
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
