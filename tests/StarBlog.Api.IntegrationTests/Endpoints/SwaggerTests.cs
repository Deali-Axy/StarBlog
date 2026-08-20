using System.Net;
using StarBlog.Api.IntegrationTests.Infrastructure;

namespace StarBlog.Api.IntegrationTests.Endpoints;

public sealed class SwaggerTests : IClassFixture<StarBlogApiApplicationFactory> {
    private readonly HttpClient _client;

    public SwaggerTests(StarBlogApiApplicationFactory factory) {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task BlogSwaggerJson_ReturnsOk() {
        var response = await _client.GetAsync("/swagger/blog/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerUi_ReturnsOk_WithoutPuttingTokenInTheRequestUrl() {
        // Swagger 页面本身应可打开，管理 API 的权限由页面内配置的 Bearer Token 控制。
        var response = await _client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    }
}
