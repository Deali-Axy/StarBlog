using StarBlog.Web.Middlewares;

namespace StarBlog.Web.Extensions;

/// <summary>
/// Swagger授权配置选项
/// </summary>
public class SwaggerAuthOptions {
    /// <summary>
    /// 是否启用Swagger访问授权，默认为true
    /// </summary>
    public bool RequireAuthentication { get; set; } = true;

    /// <summary>
    /// 是否仅在生产环境启用授权，默认为false（所有环境都启用）
    /// </summary>
    public bool OnlyInProduction { get; set; } = false;

    /// <summary>
    /// 允许的角色列表，为空则只要求认证即可
    /// </summary>
    public string[] AllowedRoles { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 自定义未授权响应消息
    /// </summary>
    public string? UnauthorizedMessage { get; set; }
}

/// <summary>
/// Swagger授权配置扩展方法
/// </summary>
public static class SwaggerAuthConfigExtensions {
    /// <summary>
    /// 配置Swagger授权选项
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection ConfigureSwaggerAuth(this IServiceCollection services,
        Action<SwaggerAuthOptions> configureOptions) {
        services.Configure(configureOptions);
        return services;
    }

    /// <summary>
    /// 使用Swagger授权中间件（带配置选项）
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="options">授权选项</param>
    /// <returns>应用程序构建器</returns>
    public static IApplicationBuilder UseSwaggerAuth(this IApplicationBuilder app, SwaggerAuthOptions? options = null) {
        if (options != null) {
            // 如果设置了仅在生产环境启用，且当前不是生产环境，则跳过授权
            if (options.OnlyInProduction) {
                var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
                if (!env.IsProduction()) {
                    return app;
                }
            }

            // 如果禁用了认证，则跳过授权
            if (!options.RequireAuthentication) {
                return app;
            }
        }

        return app.UseMiddleware<SwaggerAuthMiddleware>();
    }
}