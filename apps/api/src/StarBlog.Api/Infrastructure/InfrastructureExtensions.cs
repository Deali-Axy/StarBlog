using StarBlog.Api.Infrastructure.Background;
using StarBlog.Api.Infrastructure.Email;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Infrastructure.Storage;
using StarBlog.Api.Infrastructure.Time;

namespace StarBlog.Api.Infrastructure;

/// <summary>
/// 组合根使用的基础设施注册。
/// </summary>
public static class InfrastructureExtensions {
    /// <summary>注册持久化、存储、邮件、时钟和后台队列。</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) {
        services.AddStarBlogPersistence(configuration);
        services.Configure<EmailAccountOptions>(configuration.GetSection(EmailAccountOptions.SectionName));
        services.AddSingleton<IAppPathProvider, AspNetAppPathProvider>();
        services.AddSingleton<IFileStorage, PhysicalFileStorage>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<BackgroundTaskWorker>();
        return services;
    }
}
