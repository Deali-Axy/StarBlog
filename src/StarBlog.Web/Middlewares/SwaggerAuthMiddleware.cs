using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net;

namespace StarBlog.Web.Middlewares;

/// <summary>
/// Swagger访问授权中间件
/// 提供对Swagger UI和API文档的访问控制
/// </summary>
public class SwaggerAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SwaggerAuthMiddleware> _logger;

    public SwaggerAuthMiddleware(RequestDelegate next, ILogger<SwaggerAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// 处理HTTP请求，检查Swagger相关路径的访问权限
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>异步任务</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();
        
        // 检查是否为Swagger相关路径
        if (IsSwaggerPath(path))
        {
            // 检查用户是否已通过JWT认证
            if (!context.User.Identity?.IsAuthenticated == true)
            {
                _logger.LogWarning("未授权访问Swagger UI: {Path} from {RemoteIpAddress}", 
                    context.Request.Path, context.Connection.RemoteIpAddress);
                
                await HandleUnauthorizedAccess(context);
                return;
            }
            
            _logger.LogInformation("已授权用户访问Swagger: {User} -> {Path}", 
                context.User.Identity.Name, context.Request.Path);
        }

        await _next(context);
    }

    /// <summary>
    /// 判断是否为Swagger相关路径
    /// </summary>
    /// <param name="path">请求路径</param>
    /// <returns>是否为Swagger路径</returns>
    private static bool IsSwaggerPath(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        
        return path.StartsWith("/swagger") || 
               path.StartsWith("/api-docs");
    }

    /// <summary>
    /// 处理未授权访问
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>异步任务</returns>
    private static async Task HandleUnauthorizedAccess(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json; charset=utf-8";
        
        var response = new
        {
            error = "Unauthorized",
            message = "访问Swagger UI需要有效的JWT令牌认证",
            details = "请先通过 /Api/Auth/login 接口获取JWT令牌，然后在请求头中添加 'Authorization: Bearer {token}'",
            timestamp = DateTime.UtcNow
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Swagger授权中间件扩展方法
/// </summary>
public static class SwaggerAuthMiddlewareExtensions
{
    /// <summary>
    /// 添加Swagger授权中间件
    /// </summary>
    /// <param name="builder">应用程序构建器</param>
    /// <returns>应用程序构建器</returns>
    public static IApplicationBuilder UseSwaggerAuth(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SwaggerAuthMiddleware>();
    }
}