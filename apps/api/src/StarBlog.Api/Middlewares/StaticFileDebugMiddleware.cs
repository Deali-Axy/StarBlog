using Microsoft.Extensions.FileProviders;

namespace StarBlog.Api.Middlewares;

/// <summary>
/// 静态文件调试中间件
/// </summary>
public class StaticFileDebugMiddleware(
    RequestDelegate next,
    ILogger<StaticFileDebugMiddleware> logger,
    IWebHostEnvironment env
) {
    public async Task InvokeAsync(HttpContext context) {
        var path = context.Request.Path.Value;

        // 只记录静态文件请求
        if (path != null && (path.StartsWith("/js/") || path.StartsWith("/css/") || path.StartsWith("/lib/"))) {
            var physicalPath = Path.Combine(env.WebRootPath, path.TrimStart('/'));
            var fileExists = File.Exists(physicalPath);

            logger.LogInformation("Static file request: {Path}, Physical: {PhysicalPath}, Exists: {Exists}",
                path, physicalPath, fileExists);

            if (!fileExists) {
                logger.LogWarning("Static file not found: {Path} -> {PhysicalPath}", path, physicalPath);
            }
        }

        await next(context);

        // 记录响应状态
        if (path != null && (path.StartsWith("/js/") || path.StartsWith("/css/") || path.StartsWith("/lib/"))) {
            logger.LogInformation("Static file response: {Path}, Status: {StatusCode}",
                path, context.Response.StatusCode);
        }
    }
}