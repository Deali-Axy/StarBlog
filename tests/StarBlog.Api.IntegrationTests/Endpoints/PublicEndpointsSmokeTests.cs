using System.Net;
using StarBlog.Api.IntegrationTests.Infrastructure;
using StarBlog.Testing;

namespace StarBlog.Api.IntegrationTests.Endpoints;

public sealed class PublicEndpointsSmokeTests : IClassFixture<StarBlogApiApplicationFactory>, IAsyncLifetime {
    private readonly StarBlogApiApplicationFactory _factory;
    private readonly HttpClient _client;

    public PublicEndpointsSmokeTests(StarBlogApiApplicationFactory factory) {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync() {
        await TestDatabaseSeeder.EnsureEfCoreDatabaseAsync(_factory.Services);
        await TestDatabaseSeeder.SeedMinimalBlogDataAsync(_factory.Services);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task BlogPostList_ReturnsOk() {
        var response = await _client.GetAsync("/api/v1/posts?page=1&pageSize=1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CategoryNodes_ReturnsOk() {
        var response = await _client.GetAsync("/api/v1/categories/tree");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Theme_ReturnsOk() {
        var response = await _client.GetAsync("/api/v1/theme");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PublicLinks_ReturnsOk() {
        var response = await _client.GetAsync("/api/v1/links");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/v1/site/home")]
    [InlineData("/api/v1/site/search?keyword=Test")]
    [InlineData("/feed")]
    [InlineData("/robots.txt")]
    [InlineData("/sitemap-index.xml")]
    [InlineData("/sitemap.xml")]
    [InlineData("/sitemap-images.xml")]
    public async Task MigratedSiteEndpoints_ReturnOk(string url) {
        // 这些端点替代旧 Razor 控制器；验证 API 项目本身可以提供它们。
        var response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminAlias_ReturnsOk_ForAuthenticatedRequest() {
        // 测试工厂注入的测试身份验证器会自动提供管理员身份。
        var response = await _client.GetAsync("/api/v1/admin/comments?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/Api/BlogPost")]
    [InlineData("/Api/Admin/Posts")]
    [InlineData("/Api/Site/home")]
    public async Task LegacyRoutes_AreNotExposed(string url) {
        // 项目不保留旧 StarBlog.Web 路由别名，避免客户端继续依赖大小写不规范的历史地址。
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
