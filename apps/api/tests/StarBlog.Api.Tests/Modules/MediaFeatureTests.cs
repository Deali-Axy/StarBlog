using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EasyImageSharp;
using EasyImageSharp.PixelFormats;
using Microsoft.Extensions.DependencyInjection;
using StarBlog.Api.Infrastructure.Storage;
using StarBlog.Api.Infrastructure.Persistence;
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
        using var decodedThumb = Image.Load<Rgba32>(await thumb.Content.ReadAsByteArrayAsync());
        Assert.Equal(32, decodedThumb.Width);
        Assert.Equal(32, decodedThumb.Height);

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

    [Theory]
    [InlineData("landscape.jpg", 1500, 1000)]
    [InlineData("landscape.png", 1500, 1000)]
    [InlineData("landscape.webp", 1500, 1000)]
    [InlineData("orientation-6.jpg", 1000, 1500)]
    [InlineData("transparent.png", 16, 10)]
    [InlineData("panorama.png", 1500, 1)]
    [InlineData("animated.gif", 1, 1)]
    public async Task Upload_encodes_supported_inputs_with_correct_dimensions(string fixture, int width, int height) {
        using var form = CreatePhotoForm(await File.ReadAllBytesAsync(FixturePath(fixture)), fixture);
        using var created = await _client.PostAsync("/api/v1/photos", form);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var photo = await created.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(width, photo.GetProperty("width").GetInt32());
        Assert.Equal(height, photo.GetProperty("height").GetInt32());
        var id = photo.GetProperty("id").GetString();
        try {
            using var scope = _factory.Services.CreateScope();
            var paths = scope.ServiceProvider.GetRequiredService<IAppPathProvider>();
            var bytes = await File.ReadAllBytesAsync(Path.Combine(paths.WebRootPath, "media", "photography", photo.GetProperty("filePath").GetString()!));
            Assert.Equal(0xff, bytes[0]);
            Assert.Equal(0xd8, bytes[1]);
            using var image = Image.Load<Rgba32>(bytes);
            Assert.Equal(width, image.Width);
            Assert.Equal(height, image.Height);
            if (fixture == "transparent.png") {
                var pixel = image[0, 0];
                Assert.True(pixel.R > 245 && pixel.G > 245 && pixel.B > 245, "透明像素转 JPEG 应合成白底");
            }
            if (fixture == "animated.gif") {
                Assert.True(image[0, 0].R > 230 && image[0, 0].B < 30, "摄影 JPEG 应使用 GIF 的红色首帧");
            }
            if (fixture == "orientation-6.jpg") {
                using var thumb = await _client.GetAsync($"/api/v1/photos/{id}/thumbnail?width=200&quality=85");
                Assert.Equal(HttpStatusCode.OK, thumb.StatusCode);
                using var preview = Image.Load<Rgba32>(await thumb.Content.ReadAsByteArrayAsync());
                Assert.Equal(200, preview.Width);
                Assert.Equal(300, preview.Height);
                var topRight = preview[180, 20];
                var bottomLeft = preview[20, 280];
                Assert.True(topRight.R > 230 && topRight.B < 30, "EXIF 方向应只归一化一次");
                Assert.True(bottomLeft.B > 230 && bottomLeft.R < 30);
            }
        }
        finally {
            await _client.DeleteAsync($"/api/v1/photos/{id}");
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Invalid_or_oversized_image_returns_400_without_persisting(bool oversized) {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StarBlogDbContext>();
        var paths = scope.ServiceProvider.GetRequiredService<IAppPathProvider>();
        var count = db.Photos.Count();
        var directory = Path.Combine(paths.WebRootPath, "media", "photography");
        var files = Directory.Exists(directory) ? Directory.GetFiles(directory).Order().ToArray() : [];
        var bytes = oversized ? await File.ReadAllBytesAsync(FixturePath("oversized.png")) : "not an image"u8.ToArray();
        using var form = CreatePhotoForm(bytes, "invalid.png");
        using var response = await _client.PostAsync("/api/v1/photos", form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        if (oversized) Assert.Contains("5000 万像素", await response.Content.ReadAsStringAsync());
        Assert.Equal(count, db.Photos.Count());
        Assert.Equal(files, Directory.Exists(directory) ? Directory.GetFiles(directory).Order().ToArray() : []);
    }

    [Theory]
    [InlineData("width=0&quality=85")]
    [InlineData("width=2001&quality=85")]
    [InlineData("width=300&quality=0")]
    [InlineData("width=300&quality=101")]
    public async Task Thumbnail_rejects_out_of_range_parameters(string query) {
        using var response = await _client.GetAsync($"/api/v1/photos/absent/thumbnail?{query}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static string FixturePath(string name) => Path.Combine(AppContext.BaseDirectory, "Fixtures", "Media", name);

    private static MultipartFormDataContent CreatePhotoForm(byte[] bytes, string filename) {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent("Image compatibility"), "title");
        form.Add(new ByteArrayContent(bytes), "file", filename);
        return form;
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
