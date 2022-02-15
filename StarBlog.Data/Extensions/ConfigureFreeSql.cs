using FreeSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StarBlog.Data.Extensions;

public static class ConfigureFreeSql {
    public static void AddFreeSql(this IServiceCollection services, IConfiguration configuration) {
        var freeSql = new FreeSqlBuilder()
            .UseConnectionString(DataType.Sqlite, configuration.GetConnectionString("SQLite"))
            .UseAutoSyncStructure(false)
            .Build();

        services.AddSingleton(freeSql);

        // 仓储模式支持
        services.AddFreeRepository();
    }
}