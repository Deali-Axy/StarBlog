using DataProc.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataProc.Framework.Extensions;

public static class EFCoreItegration {
    public static void AddDefaultEFCoreItegration(this FluentConsoleApp app) {
        var connectionString = app.Configuration.GetConnectionString("SQLite")
            ?? throw new InvalidOperationException("缺少 SQLite 连接字符串配置。");
        app.Services.AddDbContext<AppDbContext>(options => {
            options.UseSqlite(connectionString);
        });
    }
}
