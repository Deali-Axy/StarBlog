using Microsoft.AspNetCore.HttpOverrides;
using SixLabors.ImageSharp.Web.DependencyInjection;
using StarBlog.Api.Hosting.Authentication;
using StarBlog.Api.Hosting.HealthChecks;
using StarBlog.Api.Hosting.OpenApi;

namespace StarBlog.Api.Hosting;

/// <summary>
/// ASP.NET Core 宿主横切能力：压缩、CORS、认证、OpenAPI、健康检查。
/// </summary>
public static class WebFrameworkExtensions {
    /// <summary>注册 Web 框架服务。Program.cs 只调用此方法与各模块 Add*。</summary>
    public static IServiceCollection AddWebFramework(this IServiceCollection services, IConfiguration configuration) {
        services.AddProblemDetails();
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddHttpClient();
        services.AddResponseCompression(options => {
            options.EnableForHttps = true;
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
        });
        services.AddCors(options => {
            options.AddDefaultPolicy(policy => {
                policy.AllowCredentials();
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.WithOrigins(
                    "http://localhost:3000",
                    "http://localhost:5173",
                    "http://localhost:8080",
                    "http://localhost:8081",
                    "https://deali.cn",
                    "https://blog.deali.cn");
            });
        });
        services.AddStarBlogAuthentication(configuration);
        services.AddStarBlogOpenApi();
        services.AddStarBlogHealthChecks();
        services.AddImageSharp();
        return services;
    }

    /// <summary>配置中间件管道中的框架部分。</summary>
    public static WebApplication UseWebFramework(this WebApplication app) {
        if (app.Environment.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        }
        else {
            app.UseExceptionHandler();
            app.UseHsts();
        }

        app.UseForwardedHeaders(new ForwardedHeadersOptions {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
        app.UseImageSharp();
        app.UseResponseCompression();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseStarBlogOpenApi();
        app.MapStarBlogHealthChecks();
        return app;
    }
}
