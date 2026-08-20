namespace StarBlog.Api.Modules.Configuration;

/// <summary>
/// Configuration 模块注册。
/// </summary>
public static class ConfigurationModule {
    public static IServiceCollection AddConfigurationModule(this IServiceCollection services) {
        services.AddScoped<ConfigurationOperations>();
        services.AddScoped<ISiteConfiguration>(provider => provider.GetRequiredService<ConfigurationOperations>());
        return services;
    }

    public static IEndpointRouteBuilder MapConfigurationModule(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapConfigurationEndpoints();
}
