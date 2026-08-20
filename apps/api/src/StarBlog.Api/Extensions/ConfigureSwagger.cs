using Microsoft.OpenApi;
using StarBlog.Api.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace StarBlog.Api.Extensions;

public static class ApiGroups {
    public const string Admin = "admin";
    public const string Auth = "auth";
    public const string Blog = "blog";
    public const string Comment = "comment";
    public const string Common = "common";
    public const string Link = "link";
    public const string Photo = "photo";
}

public static class ConfigureSwagger {
    public static readonly List<SwaggerGroup> Groups = new() {
        new SwaggerGroup(ApiGroups.Admin, "Admin APIs", "管理员相关接口"),
        new SwaggerGroup(ApiGroups.Auth, "Auth APIs", "授权接口"),
        new SwaggerGroup(ApiGroups.Blog, "Blog APIs", "博客管理接口"),
        new SwaggerGroup(ApiGroups.Comment, "Comment APIs", "评论接口"),
        new SwaggerGroup(ApiGroups.Common, "Common APIs", "通用公共接口"),
        new SwaggerGroup(ApiGroups.Link, "Link APIs", "友情链接接口"),
        new SwaggerGroup(ApiGroups.Photo, "Photo APIs", "图片管理接口")
    };

    public static void AddSwagger(this IServiceCollection services) {
        services.AddSwaggerGen(options => {
            Groups.ForEach(group => options.SwaggerDoc(group.Name, group.ToOpenApiInfo()));

            // Swagger UI 本身公开访问；受保护的 API 仍由 JWT Bearer 鉴权。
            // 使用标准 HTTP Bearer 定义后，用户在 Authorize 对话框中只需粘贴令牌本身。
            var security = new OpenApiSecurityScheme {
                Description = "请输入登录接口返回的 JWT Token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            };
            options.AddSecurityDefinition("Bearer", security);
            options.AddSecurityRequirement(doc => {
                var securityRef = new OpenApiSecuritySchemeReference("Bearer", doc, string.Empty);
                return new OpenApiSecurityRequirement { { securityRef, new List<string>() } };
            });
            options.OperationFilter<AddResponseHeadersFilter>();
            options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
            options.OperationFilter<SecurityRequirementsOperationFilter>();

            // XML注释
            var filePath = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
            options.IncludeXmlComments(filePath, true);
        });
    }

    /// <summary>
    /// 配置Swagger中间件
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    public static void UseSwaggerPkg(this IApplicationBuilder app) {
        app.UseSwagger();
        app.UseSwaggerUI(opt => {
            // 文档入口固定为 /swagger；Swagger JSON 仍位于 /swagger/{group}/swagger.json。
            opt.RoutePrefix = "swagger";
            // 模型的默认扩展深度，设置为 -1 完全隐藏模型
            opt.DefaultModelsExpandDepth(-1);
            // API文档仅展开标记
            opt.DocExpansion(DocExpansion.List);
            opt.DocumentTitle = "StarBlog APIs";
            // 分组
            Groups.ForEach(group => opt.SwaggerEndpoint($"/swagger/{group.Name}/swagger.json", group.Name));
        });
    }
}
