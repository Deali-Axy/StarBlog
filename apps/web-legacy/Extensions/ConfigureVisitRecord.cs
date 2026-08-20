using IP2Region.Net.Abstractions;
using IP2Region.Net.XDB;
using StarBlog.Infrastructure.Ip;
using StarBlog.Web.Services.VisitRecordServices;

namespace StarBlog.Web.Extensions;

public static class ConfigureVisitRecord {
    public static IServiceCollection AddVisitRecord(this IServiceCollection services) {
        services.AddScoped<VisitRecordAnalyticsService>();
        services.AddSingleton<VisitRecordQueueService>();
        services.AddHostedService<VisitRecordWorker>();
        
        var dbPath = Path.Combine(Environment.CurrentDirectory, "ip2region.xdb");
        if (File.Exists(dbPath)) {
            services.AddSingleton<ISearcher>(new Searcher(
                // 这里选择整个数据文件都缓存到内存里，性能更高，也能实现线程安全
                // 事实上 C# 版实现不缓存也是线程安全的（作者说的）
                CachePolicy.Content, dbPath
            ));
        }
        else {
            services.AddSingleton<ISearcher, FakeIpSearcher>();
        }
        
        return services;
    }
}
