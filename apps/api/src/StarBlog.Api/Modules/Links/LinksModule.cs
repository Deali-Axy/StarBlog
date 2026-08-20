namespace StarBlog.Api.Modules.Links;

/// <summary>
/// Links 模块注册。
/// </summary>
public static class LinksModule {
    public static IServiceCollection AddLinksModule(this IServiceCollection services) {
        services.AddScoped<LinkOperations>();
        services.AddScoped<IPublicLinkCatalog>(provider => provider.GetRequiredService<LinkOperations>());
        return services;
    }

    public static IEndpointRouteBuilder MapLinksModule(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapLinkEndpoints();
}
