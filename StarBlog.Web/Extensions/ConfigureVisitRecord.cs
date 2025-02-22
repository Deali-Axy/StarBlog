using StarBlog.Web.Services;

namespace StarBlog.Web.Extensions;

public static class ConfigureVisitRecord {
    public static IServiceCollection AddVisitRecord(this IServiceCollection services) {
        services.AddScoped<VisitRecordService>();
        services.AddSingleton<VisitRecordQueueService>();
        services.AddHostedService<VisitRecordWorker>();
        
        return services;
    }
}