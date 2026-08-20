using StarBlog.Content.Utils;
using StarBlog.Application.Models.Config;

namespace StarBlog.Api.Extensions; 

public static class ConfigureAppSettings {
    public static void AddSettings(this IServiceCollection services, ConfigurationManager configuration) {
        configuration.AddJsonFile("appsettings-email.json", optional: true, reloadOnChange: true);
        configuration.AddJsonFile("appsettings-monitoring.json", optional: true, reloadOnChange: true);
        // 安全配置
        services.Configure<Auth>(configuration.GetSection(nameof(Auth)));
        // 邮件配置
        services.Configure<EmailAccountConfig>(configuration.GetSection(nameof(EmailAccountConfig)));
        // 监控配置
        services.Configure<Monitoring>(configuration.GetSection(nameof(Monitoring)));
    }
}
