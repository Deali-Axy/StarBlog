using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarBlog.Api.Infrastructure.Email;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Modules.Content.Domain;

namespace StarBlog.Api.Tests.Infrastructure;

/// <summary>
/// 为每次测试创建隔离的临时 SQLite 数据库，并禁用后台 Worker。
/// </summary>
public sealed class StarBlogApiFactory : WebApplicationFactory<Program> {
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "starblog-tests", Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        Directory.CreateDirectory(_directory);
        Directory.CreateDirectory(Path.Combine(_directory, "wwwroot"));
        var dbPath = Path.Combine(_directory, "app.db");

        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={dbPath}");
        builder.UseSetting("host", "http://localhost");
        builder.UseSetting("StarBlog:Initial:host", "http://localhost");
        builder.UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "StarBlog.Api")));
        builder.UseWebRoot(Path.Combine(_directory, "wwwroot"));
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureAppConfiguration((_, configuration) => {
            configuration.AddInMemoryCollection(new Dictionary<string, string?> {
                ["host"] = "http://localhost",
                ["ConnectionStrings:Default"] = $"Data Source={dbPath}",
                ["StarBlog:Initial:host"] = "http://localhost"
            });
        });
        builder.ConfigureServices(services => {
            foreach (var descriptor in services.Where(service => service.ServiceType == typeof(IHostedService)).ToList()) {
                var name = descriptor.ImplementationType?.Name;
                if (name is "OutboxWorker" or "VisitRecordWorker" or "BackgroundTaskWorker") {
                    services.Remove(descriptor);
                }
            }

            services.AddSingleton<IEmailSender, NullEmailSender>();
            services.AddAuthentication(options => {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    /// <summary>写入一篇已发布文章，供公开接口使用。</summary>
    public async Task SeedPublishedPostAsync() {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StarBlogDbContext>();
        if (!db.Categories.Any()) {
            db.Categories.Add(new Category { Name = "测试分类", ParentId = 0, Visible = true });
            await db.SaveChangesAsync();
        }

        if (!db.Posts.Any()) {
            var categoryId = db.Categories.Select(category => category.Id).First();
            db.Posts.Add(new Post {
                Id = "testpost00000001",
                Title = "Test Post",
                Summary = "Test summary",
                Content = "# Hello\nThis is a Test article.",
                IsPublish = true,
                CategoryId = categoryId,
                CreationTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            });
            await db.SaveChangesAsync();
        }
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if (!disposing) return;
        try {
            if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }
        catch (IOException) {
        }
    }
}

/// <summary>测试中不真正发送邮件。</summary>
file sealed class NullEmailSender : IEmailSender {
    public Task SendAsync(string subject, string htmlBody, string toName, string toAddress, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
