using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StarBlog.Api.Infrastructure.Persistence;

/// <summary>
/// 设计时工厂，供 `dotnet ef` 生成 migration。
/// </summary>
public sealed class StarBlogDesignTimeDbContextFactory : IDesignTimeDbContextFactory<StarBlogDbContext> {
    /// <summary>使用环境变量 CONNECTION_STRING 或默认 SQLite 文件创建上下文。</summary>
    public StarBlogDbContext CreateDbContext(string[] args) {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                               ?? "Data Source=app.db";
        var options = new DbContextOptionsBuilder<StarBlogDbContext>()
            .UseSqlite(connectionString)
            .Options;
        return new StarBlogDbContext(options);
    }
}
