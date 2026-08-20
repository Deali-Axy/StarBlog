using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace StarBlog.Api.Hosting.OpenApi;

/// <summary>
/// OpenAPI 文档分组与 JWT Bearer 定义。
/// </summary>
public static class OpenApiExtensions {
    public static readonly (string Name, string Title, string Description)[] Groups = [
        ("admin", "Admin APIs", "管理员相关接口"),
        ("auth", "Auth APIs", "授权接口"),
        ("blog", "Blog APIs", "博客管理接口"),
        ("comment", "Comment APIs", "评论接口"),
        ("common", "Common APIs", "通用公共接口"),
        ("link", "Link APIs", "友情链接接口"),
        ("photo", "Photo APIs", "图片管理接口")
    ];

    /// <summary>注册按模块分组的 Swagger 文档。</summary>
    public static IServiceCollection AddStarBlogOpenApi(this IServiceCollection services) {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options => {
            foreach (var (name, title, description) in Groups) {
                options.SwaggerDoc(name, new OpenApiInfo { Title = title, Description = description, Version = "v1" });
            }

            var security = new OpenApiSecurityScheme {
                Description = "请输入登录接口返回的 JWT Token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            };
            options.AddSecurityDefinition("Bearer", security);
            options.AddSecurityRequirement(document => {
                var securityRef = new OpenApiSecuritySchemeReference("Bearer", document, string.Empty);
                return new OpenApiSecurityRequirement { { securityRef, new List<string>() } };
            });

            var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
            if (File.Exists(xmlPath)) {
                options.IncludeXmlComments(xmlPath, true);
            }
        });
        return services;
    }

    /// <summary>启用 Swagger UI，匿名可访问；管理 API 仍需 JWT。</summary>
    public static WebApplication UseStarBlogOpenApi(this WebApplication app) {
        app.UseSwagger();
        app.UseSwaggerUI(options => {
            options.RoutePrefix = "swagger";
            options.DefaultModelsExpandDepth(-1);
            options.DocExpansion(DocExpansion.List);
            options.DocumentTitle = "StarBlog APIs";
            foreach (var (name, _, _) in Groups) {
                options.SwaggerEndpoint($"/swagger/{name}/swagger.json", name);
            }
        });
        return app;
    }
}
