using System.Net;
using StarBlog.Api.Tests.Infrastructure;

namespace StarBlog.Api.Tests.Modules;

public sealed class PublicEndpointsTests : IClassFixture<StarBlogApiFactory> {
    private readonly HttpClient _client;

    public PublicEndpointsTests(StarBlogApiFactory factory) {
        factory.SeedPublishedPostAsync().GetAwaiter().GetResult();
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    [InlineData("/swagger/blog/swagger.json")]
    [InlineData("/swagger/index.html")]
    [InlineData("/api/v1/posts?page=1&pageSize=1")]
    [InlineData("/api/v1/categories/tree")]
    [InlineData("/api/v1/theme")]
    [InlineData("/api/v1/links")]
    [InlineData("/api/v1/site/home")]
    [InlineData("/api/v1/site/search?keyword=Test")]
    [InlineData("/feed")]
    [InlineData("/robots.txt")]
    [InlineData("/sitemap-index.xml")]
    [InlineData("/sitemap.xml")]
    [InlineData("/sitemap-images.xml")]
    public async Task Public_endpoints_return_ok(string path) {
        var response = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/Api/BlogPost")]
    [InlineData("/Api/Admin/Posts")]
    [InlineData("/Api/Site/home")]
    public async Task Legacy_routes_are_not_exposed(string path) {
        var response = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Robots_contains_sitemap() {
        var body = await _client.GetStringAsync("/robots.txt");
        Assert.Contains("Sitemap:", body);
        Assert.Contains("/sitemap.xml", body);
    }

    [Fact]
    public async Task Swagger_ui_is_html() {
        var response = await _client.GetAsync("/swagger/index.html");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.ToString() ?? string.Empty);
    }
}
