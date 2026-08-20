using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarBlog.Testing;

namespace StarBlog.Api.IntegrationTests.Infrastructure;

public sealed class StarBlogApiApplicationFactory : WebApplicationFactory<Program> {
    private readonly TempWorkspace _workspace = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        var dataDbPath = _workspace.GetPath("app.data.db");
        var logDbPath = _workspace.GetPath("app.log.db");

        var config = new Dictionary<string, string?> {
            ["host"] = "http://localhost",
            ["ConnectionStrings:SQLite"] = $"Data Source={dataDbPath}",
            ["ConnectionStrings:SQLite-Log"] = $"Data Source={logDbPath}"
        };

        builder.UseEnvironment("Testing");

        // 集成测试不应写入 Windows 事件日志；普通用户运行测试时没有创建事件源的权限，
        // 清空宿主默认日志提供程序可让测试只验证 HTTP 行为，不受操作系统权限影响。
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureAppConfiguration((_, configurationBuilder) => {
            configurationBuilder.AddInMemoryCollection(config);
        });

        builder.ConfigureServices(services => {
            foreach (var descriptor in services.Where(x => x.ServiceType == typeof(IHostedService)).ToList()) {
                var implementationType = descriptor.ImplementationType;
                if (implementationType == null) continue;
                if (implementationType.Name is "OutboxWorker" or "VisitRecordWorker") {
                    services.Remove(descriptor);
                }
            }

            services.AddAuthentication(options => {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        if (!disposing) return;
        _workspace.Dispose();
    }
}
