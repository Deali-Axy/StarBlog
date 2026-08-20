using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace StarBlog.Api.Hosting.HealthChecks;

/// <summary>
/// 注册并映射 /health、/health/live、/health/ready。
/// </summary>
public static class HealthCheckExtensions {
    public const string LiveTag = "live";
    public const string ReadyTag = "ready";

    /// <summary>注册存活与数据库就绪检查。</summary>
    public static IServiceCollection AddStarBlogHealthChecks(this IServiceCollection services) {
        services.AddHealthChecks()
            .AddCheck<LiveHealthCheck>("live", tags: [LiveTag])
            .AddCheck<DatabaseHealthCheck>("database", tags: [ReadyTag]);
        return services;
    }

    /// <summary>映射健康检查端点，未健康时返回 503。</summary>
    public static WebApplication MapStarBlogHealthChecks(this WebApplication app) {
        app.MapHealthChecks("/health/live", CreateOptions(registration => registration.Tags.Contains(LiveTag))).AllowAnonymous();
        app.MapHealthChecks("/health/ready", CreateOptions(registration => registration.Tags.Contains(ReadyTag))).AllowAnonymous();
        app.MapHealthChecks("/health", CreateOptions(_ => true)).AllowAnonymous();
        app.MapHealthChecks("/healthz", CreateOptions(_ => true)).AllowAnonymous();
        return app;
    }

    private static HealthCheckOptions CreateOptions(Func<HealthCheckRegistration, bool> predicate) => new() {
        Predicate = predicate,
        AllowCachingResponses = false,
        ResultStatusCodes = {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        },
        ResponseWriter = WriteResponseAsync
    };

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
                error = entry.Value.Exception?.Message
            })
        };
        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
