using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Modules.Comments;
using StarBlog.Api.Modules.Notifications.Domain;
using StarBlog.Api.Tests.Infrastructure;

namespace StarBlog.Api.Tests.Modules;

public sealed class ContentAndCommentsFeatureTests : IClassFixture<StarBlogApiFactory> {
    private readonly StarBlogApiFactory _factory;
    private readonly HttpClient _client;

    public ContentAndCommentsFeatureTests(StarBlogApiFactory factory) {
        _factory = factory;
        factory.SeedPublishedPostAsync().GetAwaiter().GetResult();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Admin_can_create_and_feature_a_post() {
        var created = await _client.PostAsJsonAsync("/api/v1/posts", new {
            title = "New Post",
            slug = "new-post",
            isPublish = true,
            summary = "s",
            content = "# Hi",
            categoryId = 1
        });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var post = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = post.GetProperty("id").GetString();
        Assert.False(string.IsNullOrWhiteSpace(id));

        var featured = await _client.PostAsync($"/api/v1/posts/{id}/featured-post", null);
        Assert.Equal(HttpStatusCode.OK, featured.StatusCode);

        var top = await _client.PutAsync($"/api/v1/posts/{id}/top-placement", null);
        Assert.Equal(HttpStatusCode.OK, top.StatusCode);
    }

    [Fact]
    public async Task Publication_channel_does_not_echo_secret() {
        var created = await _client.PostAsJsonAsync("/api/v1/admin/publication-channels", new {
            name = "Zhihu",
            platform = 2,
            enabled = true,
            appSecret = "super-secret"
        });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var body = await created.Content.ReadAsStringAsync();
        Assert.DoesNotContain("super-secret", body);
        var channel = JsonSerializer.Deserialize<JsonElement>(body);
        Assert.True(channel.GetProperty("hasSecret").GetBoolean());
    }

    [Fact]
    public async Task Visible_reply_enqueues_notification_with_dedup() {
        using var scope = _factory.Services.CreateScope();
        var operations = scope.ServiceProvider.GetRequiredService<CommentOperations>();
        var parentOtpResult = await operations.GenerateOtpAsync("parent@example.com", CancellationToken.None);
        Assert.False(string.IsNullOrWhiteSpace(parentOtpResult.Otp));
        await operations.AddAsync(new CommentCreateRequest {
            PostId = "testpost00000001",
            Content = "parent comment",
            UserName = "Parent",
            Email = "parent@example.com",
            EmailOtp = parentOtpResult.Otp!
        }, "127.0.0.1", "test", CancellationToken.None);

        var parentId = scope.ServiceProvider.GetRequiredService<StarBlogDbContext>().Comments
            .Where(comment => comment.Content == "parent comment")
            .Select(comment => comment.Id)
            .First();

        var replyOtpResult = await operations.GenerateOtpAsync("child@example.com", CancellationToken.None);
        var replyResult = await operations.AddAsync(new CommentCreateRequest {
            PostId = "testpost00000001",
            ParentId = parentId,
            Content = "child reply",
            UserName = "Child",
            Email = "child@example.com",
            EmailOtp = replyOtpResult.Otp!
        }, "127.0.0.1", "test", CancellationToken.None);
        var reply = replyResult.Comment;

        var db = scope.ServiceProvider.GetRequiredService<StarBlogDbContext>();
        var messages = db.OutboxMessages.AsNoTracking().ToList();
        Assert.Contains(messages, message => message.DedupKey == $"comment-reply:{reply.Id}" && message.Status == OutboxStatus.Pending);
    }

    [Fact]
    public async Task Admin_comments_list_returns_ok() {
        var response = await _client.GetAsync("/api/v1/admin/comments?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
