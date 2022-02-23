using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StarBlog.Web.Models.Config;
using StarBlog.Web.Services;

namespace StarBlog.Web.Extensions; 

public static class ConfigureAuth {
    public static void AddAuth(this IServiceCollection services, IConfiguration configuration) {
        services.AddScoped<AuthService>();
        services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                var secSettings = configuration.GetSection(nameof(SecuritySettings)).Get<SecuritySettings>();
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = secSettings.Token.Issuer,
                    ValidAudience = secSettings.Token.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secSettings.Token.Key)),
                    ClockSkew = TimeSpan.Zero
                };
            });
    }
}