using System.Diagnostics;
using FreeSql;
using FreeSql.Internal;

namespace StarBlog.Data;

public class FreeSqlFactory {
    public static IFreeSql Create(DataType dataType, string connectionString) {
        return new FreeSql.FreeSqlBuilder()
            .UseConnectionString(dataType, connectionString)
            .UseNameConvert(NameConvertType.PascalCaseToUnderscoreWithLower)
            .UseAutoSyncStructure(true) //自动同步实体结构到数据库
            .UseMonitorCommand(cmd => Trace.WriteLine(cmd.CommandText))
            .Build(); //请务必定义成 Singleton 单例模式
    }

    public static IFreeSql Create(string connectionString) {
        return Create(DataType.Sqlite, connectionString);
    }

    public static IFreeSql CreateMySql(string connectionString) {
        return Create(DataType.MySql, connectionString);
    }

    public static IFreeSql CreatePostgresSql(string connectionString) {
        return Create(DataType.PostgreSQL, connectionString);
    }
}