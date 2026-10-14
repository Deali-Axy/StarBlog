using System.Data;
using Microsoft.EntityFrameworkCore;

namespace StarBlog.Api.Infrastructure.Persistence;

/// <summary>
/// 应用 EF Core migrations。遇到重构前遗留的 FreeSql 数据库时给出明确错误。
/// </summary>
public static class StarBlogDatabase {
    /// <summary>
    /// 将已配置的数据库更新到当前 ModelSnapshot。
    /// 基线 <c>InitialCreate</c> 只能应用到空库；已有同名表时不会尝试 CREATE TABLE。
    /// </summary>
    public static async Task ApplyMigrationsAsync(StarBlogDbContext db, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(db);

        var pending = await db.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pending.Any(migration => migration.Contains("InitialCreate", StringComparison.Ordinal))
            && await TableExistsAsync(db, "anonymous_user", cancellationToken)) {
            throw new InvalidOperationException(
                "当前 SQLite 数据库已有表，但尚未应用基线 migration InitialCreate。" +
                "本次重构不升级旧的 FreeSql 数据库。本地开发请运行 `task api:db:reset`，然后重新初始化管理员。");
        }

        await db.Database.MigrateAsync(cancellationToken);
    }

    private static async Task<bool> TableExistsAsync(StarBlogDbContext db, string tableName, CancellationToken cancellationToken) {
        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) {
            await db.Database.OpenConnectionAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $name LIMIT 1";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "$name";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null and not DBNull;
    }
}
