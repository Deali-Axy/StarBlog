using FreeSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarBlog.Data.Models;

namespace StarBlog.Data.Extensions;

public static class ConfigureFreeSql {
    public static void AddFreeSql(this IServiceCollection services, IConfiguration configuration) {
        var freeSql = FreeSqlFactory.Create(configuration.GetConnectionString("SQLite"));
        // var freeSql = FreeSqlFactory.CreateMySql(configuration.GetConnectionString("MySql"));
        // var freeSql = FreeSqlFactory.CreatePostgresSql(configuration.GetConnectionString("PostgresSql"));

        services.AddSingleton(freeSql);

        // 发布渠道和投递历史是纯 API 阶段新增的表；启动时按实体创建，避免旧安装需要手工执行迁移。
        // 发布渠道与发布快照是纯 API 时代新增的持久化模型；分别同步可兼容当前
        // FreeSql 版本仅支持单一泛型参数的 SyncStructure API。
        freeSql.CodeFirst.SyncStructure<PublicationChannel>();
        freeSql.CodeFirst.SyncStructure<PostPublication>();

        // 仓储模式支持
        services.AddFreeRepository();
    }
}
