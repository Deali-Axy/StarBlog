using DataProc.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataProc.Framework.Extensions;

public static class EFCoreItegration {
    public static void AddDefaultEFCoreItegration(this FluentConsoleApp app) {
        app.Services.AddDbContext<AppDbContext>(options => {
            options.UseSqlite(app.Configuration.GetConnectionString("SQLite"));
        });
    }
}