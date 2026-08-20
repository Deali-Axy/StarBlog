using IP2Region.Net.Abstractions;
using IP2Region.Net.XDB;

namespace StarBlog.Api.Modules.Analytics;

/// <summary>
/// Analytics 模块注册。
/// </summary>
public static class AnalyticsModule {
    public static IServiceCollection AddAnalyticsModule(this IServiceCollection services) {
        services.AddSingleton<ISearcher>(_ => {
            var dbPath = Path.Combine(AppContext.BaseDirectory, "ip2region.xdb");
            return File.Exists(dbPath) ? new Searcher(CachePolicy.Content, dbPath) : new FakeIpSearcher();
        });
        services.AddSingleton<VisitRecordQueue>();
        services.AddScoped<AnalyticsOperations>();
        services.AddHostedService<VisitRecordWorker>();
        return services;
    }

    public static IEndpointRouteBuilder MapAnalyticsModule(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapAnalyticsEndpoints();
}
