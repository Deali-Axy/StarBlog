using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Modules.Content.Domain;
using StarBlog.Api.Tests.Infrastructure;

namespace StarBlog.Api.Tests.Modules;

public sealed class PublicReadFeatureTests : IClassFixture<StarBlogApiFactory> {
    private readonly StarBlogApiFactory _factory;
    private readonly HttpClient _client;

    public PublicReadFeatureTests(StarBlogApiFactory factory) {
        _factory = factory;
        factory.SeedPublishedPostAsync().GetAwaiter().GetResult();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Related_posts_exclude_self_and_only_return_published() {
        var created = await _client.PostAsJsonAsync("/api/v1/posts", new {
            title = "Related Sibling",
            slug = "related-sibling",
            isPublish = true,
            summary = "related",
            content = "body",
            categoryId = 1
        });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var sibling = await created.Content.ReadFromJsonAsync<JsonElement>();
        var siblingId = sibling.GetProperty("id").GetString();

        var response = await _factory.CreateAnonymousClient().GetAsync("/api/v1/posts/testpost00000001/related?limit=4");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<JsonElement>();
        var ids = items.EnumerateArray().Select(item => item.GetProperty("id").GetString()).ToList();
        Assert.Contains(siblingId, ids);
        Assert.DoesNotContain("testpost00000001", ids);
        Assert.All(items.EnumerateArray(), item => Assert.True(item.GetProperty("isPublish").GetBoolean()));
    }

    [Fact]
    public async Task Random_post_is_published() {
        var response = await _factory.CreateAnonymousClient().GetAsync("/api/v1/posts/random");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var post = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(post.GetProperty("isPublish").GetBoolean());
    }

    [Fact]
    public async Task Word_cloud_returns_published_counts() {
        var response = await _factory.CreateAnonymousClient().GetAsync("/api/v1/categories/word-cloud");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(items.GetArrayLength() >= 1);
        Assert.All(items.EnumerateArray(), item => Assert.True(item.GetProperty("count").GetInt32() > 0));
    }

    [Fact]
    public async Task List_posts_sorts_by_title_on_the_server() {
        var created = await _client.PostAsJsonAsync("/api/v1/posts", new {
            title = "AAA Sorted",
            slug = "aaa-sorted",
            isPublish = true,
            summary = "s",
            content = "c",
            categoryId = 1
        });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);

        var response = await _factory.CreateAnonymousClient()
            .GetAsync("/api/v1/posts?page=1&pageSize=1&sortBy=title&sortType=asc");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<JsonElement>();
        var title = page.GetProperty("items")[0].GetProperty("title").GetString();
        Assert.Equal("AAA Sorted", title);
    }

    [Fact]
    public async Task List_and_related_posts_omit_content_while_detail_keeps_it() {
        var created = await _client.PostAsJsonAsync("/api/v1/posts", new {
            title = "Slim List Body",
            slug = "slim-list-body",
            isPublish = true,
            summary = "list summary",
            content = "full markdown body that must not appear in list payloads",
            categoryId = 1
        });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var createdPost = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = createdPost.GetProperty("id").GetString();
        Assert.False(string.IsNullOrEmpty(id));

        var anon = _factory.CreateAnonymousClient();
        var list = await anon.GetAsync("/api/v1/posts?page=1&pageSize=50&search=Slim%20List%20Body");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var listPage = await list.Content.ReadFromJsonAsync<JsonElement>();
        var listed = listPage.GetProperty("items").EnumerateArray()
            .Single(item => item.GetProperty("id").GetString() == id);
        Assert.Equal("list summary", listed.GetProperty("summary").GetString());
        Assert.True(
            !listed.TryGetProperty("content", out var listedContent) ||
            listedContent.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined);

        var related = await anon.GetAsync("/api/v1/posts/testpost00000001/related?limit=12");
        Assert.Equal(HttpStatusCode.OK, related.StatusCode);
        var relatedItems = await related.Content.ReadFromJsonAsync<JsonElement>();
        Assert.All(relatedItems.EnumerateArray(), item => Assert.True(
            !item.TryGetProperty("content", out var relatedContent) ||
            relatedContent.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined));

        var detail = await anon.GetAsync($"/api/v1/posts/{id}");
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        var detailPost = await detail.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(
            "full markdown body that must not appear in list payloads",
            detailPost.GetProperty("content").GetString());
    }

    [Fact]
    public async Task List_posts_can_include_descendant_categories() {
        var parentId = 1;
        var childResponse = await _client.PostAsJsonAsync("/api/v1/categories", new {
            name = "Child Cat",
            parentId,
            visible = true
        });
        Assert.Equal(HttpStatusCode.OK, childResponse.StatusCode);
        var child = await childResponse.Content.ReadFromJsonAsync<JsonElement>();
        var childId = child.GetProperty("id").GetInt32();

        var created = await _client.PostAsJsonAsync("/api/v1/posts", new {
            title = "Nested Child Post",
            slug = "nested-child-post",
            isPublish = true,
            summary = "s",
            content = "c",
            categoryId = childId
        });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);

        var anon = _factory.CreateAnonymousClient();
        var without = await anon.GetAsync($"/api/v1/posts?categoryId={parentId}&pageSize=50");
        var withoutPage = await without.Content.ReadFromJsonAsync<JsonElement>();
        var withoutTitles = withoutPage.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("title").GetString()).ToList();
        Assert.DoesNotContain("Nested Child Post", withoutTitles);

        var with = await anon.GetAsync($"/api/v1/posts?categoryId={parentId}&includeDescendants=true&pageSize=50");
        var withPage = await with.Content.ReadFromJsonAsync<JsonElement>();
        var withTitles = withPage.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("title").GetString()).ToList();
        Assert.Contains("Nested Child Post", withTitles);
    }

    [Fact]
    public async Task Unpublished_post_is_hidden_from_anonymous_readers() {
        var created = await _client.PostAsJsonAsync("/api/v1/posts", new {
            title = "Draft Hidden",
            slug = "draft-hidden",
            isPublish = false,
            summary = "s",
            content = "secret",
            categoryId = 1
        });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var post = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = post.GetProperty("id").GetString();

        var admin = await _client.GetAsync($"/api/v1/posts/{id}");
        Assert.Equal(HttpStatusCode.OK, admin.StatusCode);

        var anon = await _factory.CreateAnonymousClient().GetAsync($"/api/v1/posts/{id}");
        Assert.Equal(HttpStatusCode.NotFound, anon.StatusCode);
    }

    [Fact]
    public async Task Published_translation_can_be_read_publicly() {
        using (var scope = _factory.Services.CreateScope()) {
            var db = scope.ServiceProvider.GetRequiredService<StarBlogDbContext>();
            db.PostTranslations.Add(new PostTranslation {
                Id = Guid.NewGuid().ToString("N"),
                PostId = "testpost00000001",
                Language = "en",
                Title = "Test Post EN",
                Summary = "EN summary",
                Content = "English body",
                CreationTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            });
            await db.SaveChangesAsync();
        }

        var anon = _factory.CreateAnonymousClient();
        var overlay = await anon.GetAsync("/api/v1/posts/testpost00000001?lang=en");
        Assert.Equal(HttpStatusCode.OK, overlay.StatusCode);
        var post = await overlay.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Test Post EN", post.GetProperty("title").GetString());

        var listed = await anon.GetAsync("/api/v1/posts/testpost00000001/translations");
        Assert.Equal(HttpStatusCode.OK, listed.StatusCode);
        var translations = await listed.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(translations.EnumerateArray(), item => item.GetProperty("language").GetString() == "en");
    }

    [Fact]
    public async Task Sitemap_and_feed_use_next_routes() {
        var sitemap = await _client.GetStringAsync("/sitemap.xml");
        Assert.Contains("/blog", sitemap);
        Assert.Contains("/photography", sitemap);
        Assert.DoesNotContain("/Blog/List", sitemap);
        Assert.DoesNotContain("/Blog/Post/", sitemap);

        var feed = await _client.GetStringAsync("/feed");
        Assert.Contains("/blog/", feed);
        Assert.DoesNotContain("/Blog/Post/", feed);
        Assert.DoesNotContain("/p/", feed);
    }

    [Fact]
    public async Task Link_exchange_rejects_empty_name() {
        var response = await _factory.CreateAnonymousClient().PostAsJsonAsync("/api/v1/site/link-exchanges", new {
            name = "",
            url = "https://example.com",
            webMaster = "webmaster",
            email = "a@example.com"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
