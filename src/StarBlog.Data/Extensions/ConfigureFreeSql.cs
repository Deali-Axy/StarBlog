using FreeSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StarBlog.Data.Extensions;

public static class ConfigureFreeSql {
    public static void AddFreeSql(this IServiceCollection services, IConfiguration configuration) {
        var connectionString = configuration.GetConnectionString("SQLite")
            ?? throw new InvalidOperationException("缺少 SQLite 连接字符串配置。");
        var freeSql = FreeSqlFactory.Create(connectionString);
        // var freeSql = FreeSqlFactory.CreateMySql(configuration.GetConnectionString("MySql"));
        // var freeSql = FreeSqlFactory.CreatePostgresSql(configuration.GetConnectionString("PostgresSql"));

        services.AddSingleton(freeSql);

        // 仓储模式支持
        services.AddFreeRepository();
    }
}
