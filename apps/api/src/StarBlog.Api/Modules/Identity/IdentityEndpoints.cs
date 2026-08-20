using Microsoft.AspNetCore.Mvc;
using StarBlog.Api.Modules.Configuration;
using StarBlog.Api.Shared;

namespace StarBlog.Api.Modules.Identity;

/// <summary>
/// 认证与首次初始化 HTTP 边界。
/// </summary>
public static class IdentityEndpoints {
    /// <summary>映射登录、当前用户和初始化端点。</summary>
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints) {
        var auth = endpoints.MapGroup("/api/v1/auth").WithTags("auth").WithGroupName("auth");
        auth.MapPost("/tokens", Login).AllowAnonymous().WithName("Login");
        endpoints.MapGet("/api/v1/users/me", GetCurrentUser)
            .RequireAuthorization()
            .WithTags("auth")
            .WithGroupName("auth")
            .WithName("GetCurrentUser");

        var init = endpoints.MapGroup("/api/v1/site/initialization").WithTags("common").WithGroupName("common");
        init.MapGet("/", GetInitializationState).AllowAnonymous().WithName("GetInitializationState");
        init.MapPost("/", Initialize).AllowAnonymous().WithName("InitializeSite");
        return endpoints;
    }

    /// <summary>用户名密码登录。</summary>
    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        IdentityOperations operations,
        CancellationToken cancellationToken) {
        var user = await operations.FindByNameAsync(request.Username, cancellationToken);
        if (user == null) return HttpErrors.Unauthorized("用户名或密码错误");
        var token = operations.Authenticate(user, request.Password);
        return token == null ? HttpErrors.Unauthorized("用户名或密码错误") : Results.Ok(token);
    }

    /// <summary>返回 JWT 中的当前用户。</summary>
    private static IResult GetCurrentUser(HttpContext context) {
        var user = IdentityOperations.GetCurrentUser(context.User);
        return user == null ? HttpErrors.NotFound("找不到用户资料") : Results.Ok(user);
    }

    /// <summary>查询是否已完成首次初始化。</summary>
    private static async Task<InitializationStateResponse> GetInitializationState(
        IdentityOperations operations,
        ISiteConfiguration configuration,
        CancellationToken cancellationToken) {
        return new InitializationStateResponse {
            IsInitialized = await operations.IsInitializedAsync(cancellationToken),
            Host = await configuration.GetAsync("host", cancellationToken),
            DefaultRender = await configuration.GetAsync("default_render", cancellationToken)
        };
    }

    /// <summary>仅允许在用户表为空时创建首个管理员。</summary>
    private static async Task<IResult> Initialize(
        [FromBody] InitializeSiteRequest request,
        IdentityOperations operations,
        CancellationToken cancellationToken) {
        var error = await operations.InitializeAsync(request, cancellationToken);
        return error == null ? Results.NoContent() : HttpErrors.BadRequest(error);
    }
}
