using System.Net;
using System.Net.Http.Json;
using StarBlog.Api.IntegrationTests.Infrastructure;
using StarBlog.Testing;

namespace StarBlog.Api.IntegrationTests.Endpoints;

/// <summary>
/// 多平台发布 API 的最小回归测试。
/// 测试身份验证器会自动注入管理员身份，因此这里同时覆盖控制器路由和授权后的正常读取路径。
/// </summary>
public sealed class PublicationEndpointsTests : IClassFixture<StarBlogApiApplicationFactory>, IAsyncLifetime {
    private readonly StarBlogApiApplicationFactory _factory;
    private readonly HttpClient _client;

    public PublicationEndpointsTests(StarBlogApiApplicationFactory factory) {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync() {
        // 发布功能依赖同一套测试数据库初始化，确保启动时的新表同步可被正常执行。
        await TestDatabaseSeeder.EnsureEfCoreDatabaseAsync(_factory.Services);
        await TestDatabaseSeeder.SeedMinimalBlogDataAsync(_factory.Services);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PublicationChannels_ReturnsOk_ForAuthenticatedAdministrator() {
        var response = await _client.GetAsync("/api/v1/admin/publication-channels");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateChannel_ReturnsOk_WithoutEchoingSecret() {
        // 使用知乎这一无需真实第三方凭证的平台，验证渠道能被持久化，同时 API 输出不会回显密钥。
        var response = await _client.PostAsJsonAsync("/api/v1/admin/publication-channels", new {
            name = $"integration-{Guid.NewGuid():N}",
            platform = 2,
            enabled = true,
            appSecret = "must-not-be-returned"
        });
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("must-not-be-returned", responseBody, StringComparison.Ordinal);
    }
}
