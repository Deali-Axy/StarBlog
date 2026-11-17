using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using StarBlog.Api.Modules.Content;
using StarBlog.Api.Modules.Content.Domain;
using StarBlog.Api.Modules.Content.Markdown;
using StarBlog.Api.Tests.Infrastructure;

namespace StarBlog.Api.Tests.Modules;

public sealed class ArticleImageTests {
    [Fact]
    public void Extracts_reference_images_skips_code_and_badges_and_resolves_legacy_paths() {
        var post = new Post { Id = "abc", Title = "Title", Content = """
            ![badge](https://img.shields.io/build)
            ```md
            ![not an image](code.png)
            ```
            ![real][photo]

            [photo]: ./实拍.assets/image.jpg

            ![duplicate](./实拍.assets/image.jpg)
            ![second](https://blog.deali.cn/media/blog/abc/second.jpg)
            """ };
        var images = ArticleImages.Candidates(post);
        Assert.Equal(2, images.Count);
        Assert.Equal("/media/blog/abc/实拍.assets/image.jpg", images[0].Url);
        Assert.Equal("real", images[0].Alt);
        Assert.Equal("/media/blog/abc/second.jpg", images[1].Url);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:image/png;base64,aaa")]
    [InlineData("//external.test/image.jpg")]
    [InlineData("../../private.png")]
    [InlineData("%2e%2e/private.png")]
    [InlineData("folder/%5c../private.png")]
    [InlineData("https://user:secret@host.test/photo.jpg")]
    public void Rejects_unsafe_image_addresses(string url) => Assert.Null(ArticleImages.ResolveUrl(url, "abc"));

    [Fact]
    public void Manual_hidden_and_translation_fallback_are_distinct() {
        var post = new Post { Id = "abc", Title = "Original", Content = "![body](body.jpg)" };
        Assert.Equal("/media/blog/abc/body.jpg", ArticleImages.Candidates(post, "Translated without images")[0].Url);
        post.CoverUrl = "/media/manual.png";
        Assert.Equal("/media/manual.png", ArticleImages.Candidates(post)[0].Url);
        Assert.Equal("/media/blog/abc/body.jpg", ArticleImages.Candidates(post)[1].Url);
        Assert.Single(ArticleImages.BodyCandidates(post));
        post.CoverHidden = true;
        Assert.Empty(ArticleImages.Candidates(post));
        Assert.Null(PostResponse.From(post, includeContent: false).Content);
    }

    [Fact]
    public async Task Cover_settings_require_auth_preserve_body_and_round_trip() {
        await using var factory = new StarBlogApiFactory();
        await factory.SeedPublishedPostAsync();
        using var admin = factory.CreateClient();
        using var anonymous = factory.CreateAnonymousClient();
        const string id = "testpost00000001";
        var before = await admin.GetFromJsonAsync<JsonElement>($"/api/v1/posts/{id}");
        var url = $"/api/v1/posts/{id}/cover";
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PutAsJsonAsync(url, new { url = "/media/cover.jpg" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PutAsJsonAsync(url, new { url = "javascript:alert(1)" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.PutAsJsonAsync(url, new { url = "/media/cover.jpg", fit = "cover" })).StatusCode);
        var after = await admin.GetFromJsonAsync<JsonElement>($"/api/v1/posts/{id}");
        Assert.Equal(before.GetProperty("content").GetString(), after.GetProperty("content").GetString());
        Assert.Equal("/media/cover.jpg", after.GetProperty("coverImages")[0].GetProperty("url").GetString());
        Assert.Equal("cover", after.GetProperty("coverFit").GetString());
        await admin.PutAsJsonAsync(url, new { hidden = true });
        after = await admin.GetFromJsonAsync<JsonElement>($"/api/v1/posts/{id}");
        Assert.Equal(0, after.GetProperty("coverImages").GetArrayLength());
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PutAsJsonAsync(url, new { fit = "stretch" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PutAsJsonAsync(url, new { url = new string('x', 2049) })).StatusCode);
        Assert.Equal(before.GetProperty("lastUpdateTime").GetString(), after.GetProperty("lastUpdateTime").GetString());
        await admin.PutAsJsonAsync(url, new { hidden = false });
        after = await admin.GetFromJsonAsync<JsonElement>($"/api/v1/posts/{id}");
        Assert.Equal(JsonValueKind.Null, after.GetProperty("coverUrl").ValueKind);
    }
}
