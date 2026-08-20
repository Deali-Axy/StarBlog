namespace StarBlog.Api.Modules.Notifications;

/// <summary>
/// Notifications 模块注册。没有 HTTP 端点，仅提供 Outbox 与邮件模板协作。
/// </summary>
public static class NotificationsModule {
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration) {
        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
        services.AddScoped<NotificationOutbox>();
        services.AddScoped<INotificationOutbox>(provider => provider.GetRequiredService<NotificationOutbox>());
        services.AddScoped<OutboxProcessor>();
        services.AddScoped<IOutboxHandler, EmailSendOutboxHandler>();
        services.AddHostedService<OutboxWorker>();
        return services;
    }

    public static IEndpointRouteBuilder MapNotificationsModule(this IEndpointRouteBuilder endpoints) => endpoints;
}
