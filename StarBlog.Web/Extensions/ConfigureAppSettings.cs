using StarBlog.Web.Models.Config;

namespace StarBlog.Web.Extensions; 

public static class ConfigureAppSettings {
    public static void AddSettings(this IServiceCollection services, IConfiguration configuration) {
        // 安全配置
        services.Configure<SecuritySettings>(configuration.GetSection(nameof(SecuritySettings)));
    }
}