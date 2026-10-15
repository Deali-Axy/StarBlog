using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Modules.Content.Domain;
using StarBlog.Api.Tests.Infrastructure;

namespace StarBlog.Api.Tests.Modules;

public sealed class LanguageReadTests : IClassFixture<StarBlogApiFactory> {
    private readonly StarBlogApiFactory _factory;

    public LanguageReadTests(StarBlogApiFactory factory) {
        _factory = factory;
        factory.SeedPublishedPostAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Public_reads_use_selected_translations_and_fall_back_without_changing_originals() {
        using (var scope = _factory.Services.CreateScope()) {
            var db = scope.ServiceProvider.GetRequiredService<StarBlogDbContext>();
            var translated = new Post {
                Id = "language00000001", Slug = "translated-article", Title = "星云文章",
                Summary = "中文摘要", Content = "# 中文正文", IsPublish = true, CategoryId = 1,
                CreationTime = DateTime.UtcNow, LastUpdateTime = DateTime.UtcNow,
                Translations = [new PostTranslation {
                    Id = "language-translation", Language = "en", Title = "Nebula article",
                    Summary = "English summary", Content = "# Nebula heading\nEnglish body"
                }]
            };
            db.Posts.Add(translated);
            db.FeaturedPosts.Add(new FeaturedPost { Post = translated });
            db.TopPosts.Add(new TopPost { Post = translated });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAnonymousClient();
        var list = await client.GetFromJsonAsync<JsonElement>("/api/v1/posts?lang=en&pageSize=100");
        var translatedCard = list.GetProperty("items").EnumerateArray().Single(post => post.GetProperty("id").GetString() == "language00000001");
        Assert.Equal("Nebula article", translatedCard.GetProperty("title").GetString());
        Assert.Equal("English summary", translatedCard.GetProperty("summary").GetString());
        // An article without an English translation still appears with its original content.
        Assert.Contains(list.GetProperty("items").EnumerateArray(), post => post.GetProperty("title").GetString() == "Test Post");

        var filtered = await client.GetFromJsonAsync<JsonElement>("/api/v1/posts?lang=en&search=Nebula&sortBy=title&sortType=asc");
        Assert.Equal("language00000001", filtered.GetProperty("items")[0].GetProperty("id").GetString());

        var home = await client.GetFromJsonAsync<JsonElement>("/api/v1/site/home?lang=en");
        Assert.Equal("Nebula article", home.GetProperty("topPost").GetProperty("title").GetString());
        Assert.Contains(home.GetProperty("featuredPosts").EnumerateArray(), post => post.GetProperty("title").GetString() == "Nebula article");

        var search = await client.GetFromJsonAsync<JsonElement>("/api/v1/site/search?keyword=Nebula&lang=en");
        Assert.Single(search.EnumerateArray());
        Assert.Contains("<mark>Nebula</mark>", search[0].GetProperty("highlightedTitle").GetString());
        Assert.Contains("<mark>Nebula</mark>", search[0].GetProperty("highlightedSnippet").GetString());

        var related = await client.GetFromJsonAsync<JsonElement>("/api/v1/posts/testpost00000001/related?lang=en&limit=12");
        Assert.Contains(related.EnumerateArray(), post => post.GetProperty("title").GetString() == "Nebula article");

        var translatedPost = await client.GetFromJsonAsync<JsonElement>("/api/v1/posts/language00000001?lang=en");
        Assert.Equal("Nebula article", translatedPost.GetProperty("title").GetString());
        Assert.Equal("en", translatedPost.GetProperty("language").GetString());
        Assert.Equal("Nebula heading", translatedPost.GetProperty("toc")[0].GetProperty("text").GetString());
        foreach (var suffix in new[] { "", "?lang=fr" }) {
            var original = await client.GetFromJsonAsync<JsonElement>($"/api/v1/posts/language00000001{suffix}");
            Assert.Equal("星云文章", original.GetProperty("title").GetString());
            Assert.Equal("# 中文正文", original.GetProperty("content").GetString());
            Assert.Equal(JsonValueKind.Null, original.GetProperty("language").ValueKind);
        }
    }

    [Fact]
    public async Task Language_selection_does_not_expose_draft_translations() {
        using (var scope = _factory.Services.CreateScope()) {
            var db = scope.ServiceProvider.GetRequiredService<StarBlogDbContext>();
            var draft = new Post {
                Id = "languagedraft001", Title = "草稿", IsPublish = false, CategoryId = 1,
                Translations = [new PostTranslation { Id = "draft-translation", Language = "en", Title = "Secret translation", Content = "Secret body" }]
            };
            db.Posts.Add(draft);
            db.FeaturedPosts.Add(new FeaturedPost { Post = draft });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAnonymousClient();
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/v1/posts/languagedraft001?lang=en")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/v1/posts/languagedraft001/translations")).StatusCode);
        var list = await client.GetStringAsync("/api/v1/posts?lang=en");
        var home = await client.GetStringAsync("/api/v1/site/home?lang=en");
        var search = await client.GetStringAsync("/api/v1/site/search?keyword=Secret&lang=en");
        Assert.DoesNotContain("Secret translation", list);
        Assert.DoesNotContain("Secret translation", home);
        Assert.Equal("[]", search);
    }
}
