namespace StarBlog.Api.Modules.Media;

/// <summary>
/// Media 模块注册。
/// </summary>
public static class MediaModule {
    public static IServiceCollection AddMediaModule(this IServiceCollection services) {
        services.AddScoped<MediaOperations>();
        services.AddScoped<IPhotoCatalog>(provider => provider.GetRequiredService<MediaOperations>());
        return services;
    }

    public static IEndpointRouteBuilder MapMediaModule(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapMediaEndpoints();
}
