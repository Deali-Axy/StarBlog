namespace StarBlog.Api.Modules.Site;

/// <summary>
/// Site 模块注册。
/// </summary>
public static class SiteModule {
    public static IServiceCollection AddSiteModule(this IServiceCollection services) {
        services.AddScoped<SiteOperations>();
        services.AddScoped<SiteResourceOperations>();
        return services;
    }

    public static IEndpointRouteBuilder MapSiteModule(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapSiteEndpoints();
}
