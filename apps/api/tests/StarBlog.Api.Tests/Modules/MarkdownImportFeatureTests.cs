using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using StarBlog.Api.Tests.Infrastructure;

namespace StarBlog.Api.Tests.Modules;

/// <summary>
/// Markdown zip 导入：分类来自文件夹，正文图片被复制并改写路径。
/// </summary>
public sealed class MarkdownImportFeatureTests : IClassFixture<StarBlogApiFactory> {
    private readonly HttpClient _client;

    public MarkdownImportFeatureTests(StarBlogApiFactory factory) {
        factory.SeedPublishedPostAsync().GetAwaiter().GetResult();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Zip_import_creates_category_post_and_rewrites_image() {
        await using var zip = BuildZip();
        using var form = new MultipartFormDataContent();
        var file = new StreamContent(zip);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        form.Add(file, "file", "posts.zip");

        var imported = await _client.PostAsync("/api/v1/posts/imports", form);
        Assert.Equal(HttpStatusCode.OK, imported.StatusCode);
        var result = await imported.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(result.GetProperty("postsImported").GetInt32() >= 1);
        var postId = result.GetProperty("postIds").EnumerateArray().First().GetString();
        Assert.False(string.IsNullOrWhiteSpace(postId));

        var post = await _client.GetFromJsonAsync<JsonElement>($"/api/v1/posts/{postId}");
        Assert.Equal("Imported Title", post.GetProperty("title").GetString());
        Assert.Contains($"/media/blog/{postId}/cover.png", post.GetProperty("content").GetString());

        var tree = await _client.GetFromJsonAsync<JsonElement>("/api/v1/categories/tree");
        Assert.Contains(tree.EnumerateArray(), node => node.GetProperty("text").GetString() == "DotNet");
    }

    [Fact]
    public async Task Post_image_upload_returns_url() {
        var created = await _client.PostAsJsonAsync("/api/v1/posts", new {
            title = "With Image",
            isPublish = true,
            summary = "s",
            content = "body",
            categoryId = 1
        });
        var post = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = post.GetProperty("id").GetString();

        using var form = new MultipartFormDataContent();
        var image = new ByteArrayContent(Encoding.UTF8.GetBytes("not-a-real-image"));
        image.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(image, "file", "note.png");
        var uploaded = await _client.PostAsync($"/api/v1/posts/{id}/images", form);
        Assert.Equal(HttpStatusCode.OK, uploaded.StatusCode);
        var payload = await uploaded.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains($"/media/blog/{id}/", payload.GetProperty("url").GetString());
    }

    /// <summary>构造含分类目录、Markdown 和同目录图片的 zip。</summary>
    private static MemoryStream BuildZip() {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true)) {
            var markdown = archive.CreateEntry("DotNet/hello.md");
            using (var writer = new StreamWriter(markdown.Open())) {
                writer.Write("# Imported Title\n\nSee ![cover](cover.png)\n");
            }

            var image = archive.CreateEntry("DotNet/cover.png");
            using (var output = image.Open()) {
                var png = Convert.FromBase64String(
                    "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");
                output.Write(png);
            }
        }

        stream.Position = 0;
        return stream;
    }
}
