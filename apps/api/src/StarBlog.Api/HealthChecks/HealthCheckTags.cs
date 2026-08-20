namespace StarBlog.Api.HealthChecks;

/// <summary>
/// Health Check 标签：
/// - Live：存活探针（不检查外部依赖）
/// - Ready：就绪探针（检查关键依赖）
/// </summary>
public static class HealthCheckTags {
    public const string Live = "live";
    public const string Ready = "ready";
}

