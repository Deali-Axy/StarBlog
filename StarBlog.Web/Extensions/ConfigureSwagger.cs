using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace StarBlog.Web.Extensions;

public static class ConfigureSwagger {
    public static void AddSwagger(this IServiceCollection services) {
        services.AddSwaggerGen(options => {
            var security = new OpenApiSecurityScheme {
                Description = "JWT模式授权，请输入 \"Bearer {Token}\" 进行身份验证",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            };
            options.AddSecurityDefinition("oauth2", security);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { { security, new List<string>() } });
            options.OperationFilter<AddResponseHeadersFilter>();
            options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
            options.OperationFilter<SecurityRequirementsOperationFilter>();

            var filePath = Path.Combine(System.AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
            options.IncludeXmlComments(filePath, true);
        });
    }
}