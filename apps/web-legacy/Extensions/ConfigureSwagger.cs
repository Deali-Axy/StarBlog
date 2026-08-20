using Microsoft.OpenApi;
using StarBlog.Web.Models;
using StarBlog.Web.Middlewares;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace StarBlog.Web.Extensions;

public static class ApiGroups {
    public const string Admin = "admin";
    public const string Auth = "auth";
    public const string Blog = "blog";
    public const string Comment = "comment";
    public const string Common = "common";
    public const string Link = "link";
    public const string Photo = "photo";
    public const string Test = "test";
}

public static class ConfigureSwagger {
    public static readonly List<SwaggerGroup> Groups = new() {
        new SwaggerGroup(ApiGroups.Admin, "Admin APIs", "管理员相关接口"),
        new SwaggerGroup(ApiGroups.Auth, "Auth APIs", "授权接口"),
        new SwaggerGroup(ApiGroups.Blog, "Blog APIs", "博客管理接口"),
        new SwaggerGroup(ApiGroups.Comment, "Comment APIs", "评论接口"),
        new SwaggerGroup(ApiGroups.Common, "Common APIs", "通用公共接口"),
        new SwaggerGroup(ApiGroups.Link, "Link APIs", "友情链接接口"),
        new SwaggerGroup(ApiGroups.Photo, "Photo APIs", "图片管理接口"),
        new SwaggerGroup(ApiGroups.Test, "Test APIs", "测试接口")
    };

    public static void AddSwagger(this IServiceCollection services) {
        services.AddSwaggerGen(options => {
            Groups.ForEach(group => options.SwaggerDoc(group.Name, group.ToOpenApiInfo()));

            // 开启小绿锁
            var security = new OpenApiSecurityScheme {
                Description = "JWT模式授权，请输入 \"Bearer {Token}\" 进行身份验证",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            };
            options.AddSecurityDefinition("oauth2", security);
            options.AddSecurityRequirement(doc => {
                var securityRef = new OpenApiSecuritySchemeReference("oauth2", doc, string.Empty);
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
    /// <param name="requireAuth">是否需要授权访问，默认为true</param>
    public static void UseSwaggerPkg(this IApplicationBuilder app, bool requireAuth = true) {
        // 如果需要授权，则添加Swagger授权中间件
        if (requireAuth) {
            app.UseSwaggerAuth();
        }
        
        app.UseSwagger();
        app.UseSwaggerUI(opt => {
            opt.RoutePrefix = "api-docs/swagger";
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
