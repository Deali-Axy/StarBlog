using StarBlog.Web.Services;

namespace StarBlog.Web.Extensions;

public static class ConfigureVisitRecord {
    public static IServiceCollection AddVisitRecord(this IServiceCollection services) {
        services.AddScoped<VisitRecordQueueService>();
        services.AddScoped<VisitRecordService>();
        services.AddHostedService<VisitRecordWorker>();
        
        return services;
    }
}