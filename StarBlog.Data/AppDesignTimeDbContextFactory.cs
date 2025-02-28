using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StarBlog.Data; 

public class AppDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext> {
    public AppDbContext CreateDbContext(string[] args) {
        var builder = new DbContextOptionsBuilder<AppDbContext>();

        var connStr = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        if (connStr == null) {
            var dbpath = Path.Combine(Environment.CurrentDirectory, "app.log.db");
            connStr = $"Data Source={dbpath};";
        }

        builder.UseSqlite(connStr);
        return new AppDbContext(builder.Options);
    }
}