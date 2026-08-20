using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using StarBlog.Api.IntegrationTests.Infrastructure;
using StarBlog.Data.Models;
using StarBlog.Testing;

namespace StarBlog.Api.IntegrationTests.Endpoints;

/// <summary>
/// 验证空 SQLite 数据库可以创建首个管理员，并立即使用该账号登录。
/// 每个测试类使用独立的临时数据库，因此不会污染其他集成测试。
/// </summary>
public sealed class InitializationEndpointsTests : IClassFixture<StarBlogApiApplicationFactory>, IAsyncLifetime {
    private readonly StarBlogApiApplicationFactory _factory;
    private readonly HttpClient _client;

    public InitializationEndpointsTests(StarBlogApiApplicationFactory factory) {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync() {
        await TestDatabaseSeeder.EnsureEfCoreDatabaseAsync(_factory.Services);

        // 明确清空本测试工厂的用户表，保证用例不依赖并行测试或 FreeSql 仓储缓存的创建顺序。
        using var scope = _factory.Services.CreateScope();
        var freeSql = scope.ServiceProvider.GetRequiredService<IFreeSql>();
        await freeSql.Delete<User>().Where("1 = 1").ExecuteAffrowsAsync();
    }
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task EmptyDatabase_CanCreateAdministrator_AndLogin() {
        var before = await _client.GetStringAsync("/api/v1/site/initialization");
        Assert.False(ReadInitializationState(before));

        var initializeResponse = await _client.PostAsJsonAsync("/api/v1/site/initialization", new {
            username = "first-admin",
            password = "ChangeMe-123!",
            host = "http://localhost:5173",
            defaultRender = "frontend"
        });
        Assert.Equal(HttpStatusCode.OK, initializeResponse.StatusCode);

        var after = await _client.GetStringAsync("/api/v1/site/initialization");
        Assert.True(ReadInitializationState(after));

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/tokens", new {
            username = "first-admin",
            password = "ChangeMe-123!"
        });
        var loginBody = await loginResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.Contains("token", loginBody, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>按统一 ApiResponse 的 data 字段读取状态，避免依赖 JSON 属性顺序或空白格式。</summary>
    private static bool ReadInitializationState(string json) {
        using var document = JsonDocument.Parse(json);
        return document.RootElement
            .GetProperty("data")
            .GetProperty("isInitialized")
            .GetBoolean();
    }
}
