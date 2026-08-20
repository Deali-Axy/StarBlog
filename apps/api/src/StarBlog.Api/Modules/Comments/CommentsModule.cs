namespace StarBlog.Api.Modules.Comments;

/// <summary>
/// Comments 模块注册。
/// </summary>
public static class CommentsModule {
    public static IServiceCollection AddCommentsModule(this IServiceCollection services) {
        services.AddSingleton<CommentFilter>();
        services.AddScoped<CommentOperations>();
        return services;
    }

    public static IEndpointRouteBuilder MapCommentsModule(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapCommentEndpoints();
}
