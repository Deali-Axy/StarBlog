using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Modules.Notifications.Domain;
using StarBlog.Api.Tests.Infrastructure;

namespace StarBlog.Api.Tests.Modules;

public sealed class LinksFeatureTests : IClassFixture<StarBlogApiFactory> {
    private readonly StarBlogApiFactory _factory;
    private readonly HttpClient _client;

    public LinksFeatureTests(StarBlogApiFactory factory) {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Admin_can_crud_links_and_public_list_hides_invisible() {
        var created = await _client.PostAsJsonAsync("/api/v1/admin/links", new {
            name = "Example",
            description = "desc",
            url = "https://example.com",
            visible = true
        });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var link = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = link.GetProperty("id").GetInt32();

        var publicLinks = await _client.GetFromJsonAsync<JsonElement>("/api/v1/links");
        Assert.Contains(publicLinks.EnumerateArray(), item => item.GetProperty("id").GetInt32() == id);

        await _client.PutAsJsonAsync($"/api/v1/admin/links/{id}", new {
            name = "Example",
            description = "desc",
            url = "https://example.com",
            visible = false
        });
        var hidden = await _client.GetFromJsonAsync<JsonElement>("/api/v1/links");
        Assert.DoesNotContain(hidden.EnumerateArray(), item => item.GetProperty("id").GetInt32() == id);

        var deleted = await _client.DeleteAsync($"/api/v1/admin/links/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
    }

    [Fact]
    public async Task Link_exchange_approval_creates_visible_link_and_outbox_message() {
        var apply = await _client.PostAsJsonAsync("/api/v1/site/link-exchanges", new {
            name = "Friend Blog",
            description = "nice site",
            url = "https://friend.example",
            webMaster = "Ada",
            email = "ada@example.com"
        });
        Assert.Equal(HttpStatusCode.OK, apply.StatusCode);
        var application = await apply.Content.ReadFromJsonAsync<JsonElement>();
        var id = application.GetProperty("id").GetInt32();

        var approval = await _client.PatchAsJsonAsync($"/api/v1/admin/link-exchange-requests/{id}/approval", new { reason = "ok" });
        Assert.Equal(HttpStatusCode.OK, approval.StatusCode);

        var publicLinks = await _client.GetFromJsonAsync<JsonElement>("/api/v1/links");
        Assert.Contains(publicLinks.EnumerateArray(), item => item.GetProperty("name").GetString() == "Friend Blog");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StarBlogDbContext>();
        Assert.Contains(db.OutboxMessages, message => message.Type == "email.send" && message.Status == OutboxStatus.Pending);

        var again = await _client.PatchAsJsonAsync($"/api/v1/admin/link-exchange-requests/{id}/approval", new { reason = "ok" });
        Assert.Equal(HttpStatusCode.OK, again.StatusCode);
        Assert.Equal(1, db.OutboxMessages.Count(message => message.DedupKey != null && message.DedupKey.Contains("accept")));
    }
}
