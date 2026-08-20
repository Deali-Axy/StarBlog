using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using StarBlog.Api.Infrastructure.Storage;
using StarBlog.Api.Tests.Infrastructure;

namespace StarBlog.Api.Tests.Modules;

/// <summary>
/// 图片上传、精选、缩略图以及删除时的文件补偿。
/// </summary>
public sealed class MediaFeatureTests : IClassFixture<StarBlogApiFactory> {
    private static readonly byte[] TinyPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private readonly StarBlogApiFactory _factory;
    private readonly HttpClient _client;

    public MediaFeatureTests(StarBlogApiFactory factory) {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Upload_creates_file_and_delete_removes_it() {
        using var form = CreatePhotoForm("Harbor", "Shanghai");
        var created = await _client.PostAsync("/api/v1/photos", form);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var photo = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = photo.GetProperty("id").GetString();
        var filePath = photo.GetProperty("filePath").GetString();
        Assert.False(string.IsNullOrWhiteSpace(id));
        Assert.EndsWith(".jpg", filePath, StringComparison.OrdinalIgnoreCase);

        using var scope = _factory.Services.CreateScope();
        var paths = scope.ServiceProvider.GetRequiredService<IAppPathProvider>();
        var physical = Path.Combine(paths.WebRootPath, "media", "photography", filePath!);
        Assert.True(File.Exists(physical), "上传成功后原图应落在 wwwroot/media/photography");

        var thumb = await _client.GetAsync($"/api/v1/photos/{id}/thumbnail?width=32&quality=80");
        Assert.Equal(HttpStatusCode.OK, thumb.StatusCode);
        Assert.Equal("image/jpeg", thumb.Content.Headers.ContentType?.MediaType);

        var featured = await _client.PostAsync($"/api/v1/photos/{id}/featured-photo", null);
        Assert.Equal(HttpStatusCode.OK, featured.StatusCode);
        var featuredList = await _client.GetFromJsonAsync<JsonElement>("/api/v1/featured-photos");
        Assert.Contains(featuredList.EnumerateArray(), item => item.GetProperty("id").GetString() == id);

        var deleted = await _client.DeleteAsync($"/api/v1/photos/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.False(File.Exists(physical), "删除记录时应同时删除物理文件，避免悬挂媒体");
    }

    [Fact]
    public async Task Missing_file_returns_bad_request() {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("No File"), "title");
        var response = await _client.PostAsync("/api/v1/photos", form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>构造最小 PNG 上传表单；服务端会转成 JPEG。</summary>
    private static MultipartFormDataContent CreatePhotoForm(string title, string location) {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(title), "title");
        form.Add(new StringContent(location), "location");
        var image = new ByteArrayContent(TinyPng);
        image.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(image, "file", "tiny.png");
        return form;
    }
}
