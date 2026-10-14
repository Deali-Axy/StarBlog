using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StarBlog.Api.Infrastructure.Persistence;

namespace StarBlog.Api.Tests.Infrastructure;

public sealed class DatabaseMigrationTests {
    [Fact]
    public async Task ApplyMigrations_on_empty_database_records_initial_create() {
        await using var session = await MigrationTestDatabase.CreateAsync();
        await StarBlogDatabase.ApplyMigrationsAsync(session.Db);

        var applied = (await session.Db.Database.GetAppliedMigrationsAsync()).ToList();
        Assert.Contains("20260820110115_InitialCreate", applied);
        Assert.Empty(await session.Db.Users.ToListAsync());
    }

    [Fact]
    public async Task ApplyMigrations_rejects_leftover_freesql_database() {
        await using var session = await MigrationTestDatabase.CreateAsync(createFile: true);
        await using (var connection = new SqliteConnection(session.ConnectionString)) {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE "anonymous_user" (
                  "created_time" DATETIME NOT NULL,
                  "updated_time" DATETIME NOT NULL,
                  "is_deleted" BOOLEAN NOT NULL,
                  "id" NVARCHAR(255) NOT NULL,
                  PRIMARY KEY ("id")
                );
                """;
            await command.ExecuteNonQueryAsync();
        }

        await using var db = session.CreateContext();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => StarBlogDatabase.ApplyMigrationsAsync(db));
        Assert.Contains("task api:db:reset", error.Message, StringComparison.Ordinal);
    }
}

file sealed class MigrationTestDatabase : IAsyncDisposable {
    private readonly string _directory;

    private MigrationTestDatabase(string directory, string path) {
        _directory = directory;
        ConnectionString = $"Data Source={path}";
        Db = CreateContext();
    }

    public string ConnectionString { get; }

    public StarBlogDbContext Db { get; }

    public static Task<MigrationTestDatabase> CreateAsync(bool createFile = false) {
        var directory = Path.Combine(Path.GetTempPath(), "starblog-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "app.db");
        if (createFile) {
            using var _ = File.Create(path);
        }

        return Task.FromResult(new MigrationTestDatabase(directory, path));
    }

    public StarBlogDbContext CreateContext() {
        var options = new DbContextOptionsBuilder<StarBlogDbContext>()
            .UseSqlite(ConnectionString)
            .Options;
        return new StarBlogDbContext(options);
    }

    public async ValueTask DisposeAsync() {
        await Db.DisposeAsync();
        try {
            if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }
        catch (IOException) {
        }
    }
}
