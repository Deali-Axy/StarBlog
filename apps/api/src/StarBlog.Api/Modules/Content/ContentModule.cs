namespace StarBlog.Api.Modules.Content;

/// <summary>
/// Content 模块注册。
/// </summary>
public static class ContentModule {
    public static IServiceCollection AddContentModule(this IServiceCollection services, IConfiguration configuration) {
        services.Configure<TranslationOptions>(configuration.GetSection(TranslationOptions.SectionName));
        services.AddScoped<ContentOperations>();
        services.AddScoped<IPublishedContentQueries>(provider => provider.GetRequiredService<ContentOperations>());
        services.AddScoped<MarkdownPostImporter>();
        services.AddScoped<PublicationOperations>();
        services.AddScoped<TranslationOperations>();
        return services;
    }

    public static IEndpointRouteBuilder MapContentModule(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapContentEndpoints();
}
