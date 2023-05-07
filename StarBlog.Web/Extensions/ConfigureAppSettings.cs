using StarBlog.Share.Utils;
using StarBlog.Web.Models.Config;

namespace StarBlog.Web.Extensions; 

public static class ConfigureAppSettings {
    public static void AddSettings(this IServiceCollection services, ConfigurationManager configuration) {
        configuration.AddJsonFile("appsettings-email.json", optional: true, reloadOnChange: true);
        // 安全配置
        services.Configure<Auth>(configuration.GetSection(nameof(Auth)));
        // 邮件配置
        services.Configure<EmailAccountConfig>(configuration.GetSection(nameof(EmailAccountConfig)));
    }
}