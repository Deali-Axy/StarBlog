using Microsoft.EntityFrameworkCore;

namespace StarBlog.Api.Infrastructure.Persistence;

/// <summary>
/// 注册 StarBlogDbContext 与 SQLite。
/// </summary>
public static class PersistenceExtensions {
    /// <summary>使用 ConnectionStrings:Default，缺省时回退到 app.db。</summary>
    public static IServiceCollection AddStarBlogPersistence(this IServiceCollection services, IConfiguration configuration) {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? "Data Source=app.db";

        services.AddDbContext<StarBlogDbContext>(options => options.UseSqlite(connectionString));
        return services;
    }
}
