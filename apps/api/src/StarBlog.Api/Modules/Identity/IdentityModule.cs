using StarBlog.Api.Modules.Identity.Security;

namespace StarBlog.Api.Modules.Identity;

/// <summary>
/// Identity 模块的服务注册与 endpoint 映射。
/// </summary>
public static class IdentityModule {
    /// <summary>注册密码保护与认证用例。</summary>
    public static IServiceCollection AddIdentityModule(this IServiceCollection services) {
        services.AddSingleton<PasswordProtector>();
        services.AddScoped<IdentityOperations>();
        return services;
    }

    /// <summary>映射认证与初始化路由。</summary>
    public static IEndpointRouteBuilder MapIdentityModule(this IEndpointRouteBuilder endpoints) {
        return endpoints.MapIdentityEndpoints();
    }
}
