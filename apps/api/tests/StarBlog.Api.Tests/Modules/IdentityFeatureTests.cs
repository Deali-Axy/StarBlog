using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using StarBlog.Api.Tests.Infrastructure;

namespace StarBlog.Api.Tests.Modules;

public sealed class IdentityFeatureTests {
    [Fact]
    public async Task Empty_database_can_create_administrator_and_login() {
        await using var factory = new StarBlogApiFactory();
        var client = factory.CreateClient();

        var state = await client.GetFromJsonAsync<JsonElement>("/api/v1/site/initialization");
        Assert.False(state.GetProperty("isInitialized").GetBoolean());

        var initialize = await client.PostAsJsonAsync("/api/v1/site/initialization", new {
            username = "admin",
            password = "password123",
            host = "http://localhost",
            defaultRender = "frontend"
        });
        Assert.Equal(HttpStatusCode.NoContent, initialize.StatusCode);

        var initialized = await client.GetFromJsonAsync<JsonElement>("/api/v1/site/initialization");
        Assert.True(initialized.GetProperty("isInitialized").GetBoolean());

        var login = await client.PostAsJsonAsync("/api/v1/auth/tokens", new { username = "admin", password = "password123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = await login.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrWhiteSpace(token.GetProperty("token").GetString()));

        var rejected = await client.PostAsJsonAsync("/api/v1/site/initialization", new {
            username = "other",
            password = "password123",
            host = "http://localhost",
            defaultRender = "frontend"
        });
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
    }

    [Fact]
    public async Task Login_rejects_wrong_password() {
        await using var factory = new StarBlogApiFactory();
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/v1/site/initialization", new {
            username = "admin",
            password = "password123",
            host = "http://localhost",
            defaultRender = "frontend"
        });

        var login = await client.PostAsJsonAsync("/api/v1/auth/tokens", new { username = "admin", password = "wrong-password" });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }
}
