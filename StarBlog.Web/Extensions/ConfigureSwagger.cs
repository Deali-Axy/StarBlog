using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace StarBlog.Web.Extensions;

public static class ConfigureSwagger {
    public static void AddSwagger(this IServiceCollection services) {
        services.AddSwaggerGen(options => {
            options.SwaggerDoc("common", new OpenApiInfo {
                Version = "v1",
                Title = "Common APIs",
                Description = "通用公共接口"
            });
            options.SwaggerDoc("auth", new OpenApiInfo {
                Version = "v1",
                Title = "Auth APIs",
                Description = "授权接口"
            });
            options.SwaggerDoc("blog", new OpenApiInfo {
                Version = "v1",
                Title = "Blog APIs",
                Description = "博客管理接口"
            });
            options.SwaggerDoc("test", new OpenApiInfo {
                Version = "v1",
                Title = "Test APIs",
                Description = "测试接口"
            });

            // 开启小绿锁
            var security = new OpenApiSecurityScheme {
                Description = "JWT模式授权，请输入 \"Bearer {Token}\" 进行身份验证",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            };
            options.AddSecurityDefinition("oauth2", security);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement {{security, new List<string>()}});
            options.OperationFilter<AddResponseHeadersFilter>();
            options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
            options.OperationFilter<SecurityRequirementsOperationFilter>();

            // XML注释
            var filePath = Path.Combine(System.AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
            options.IncludeXmlComments(filePath, true);
        });
    }
}