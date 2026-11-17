using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace StarBlog.E2ETests;

public sealed class ArticleIllustrationTests {
    public static IEnumerable<object[]> LayoutCases() {
        foreach (var width in new[] { 375, 768, 1440 })
        foreach (var theme in new[] { "light", "dark" }) yield return [width, theme];
    }

    private static async Task<IBrowser> LaunchAsync(IPlaywright playwright) => await playwright.Chromium.LaunchAsync(new() {
        Headless = true,
        ExecutablePath = Environment.GetEnvironmentVariable("E2E_CHROMIUM_EXECUTABLE_PATH")
    });

    [Theory]
    [MemberData(nameof(LayoutCases))]
    public async Task Editorial_illustrations_load_without_overflow_and_compact_view_stays_textual(int width, string theme) {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = width, Height = 1200 } });
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);
        await page.AddInitScriptAsync($"localStorage.setItem('theme', '{theme}'); localStorage.setItem('starblog-cover-mode', 'single')");
        await page.GotoAsync($"{baseUrl}/blog?bv=editorial");
        var first = page.Locator("[data-blog-article]").First;
        await Expect(first.Locator("[data-article-cover]")).ToHaveAttributeAsync("data-cover-mode", "single");
        var cover = first.Locator("[data-article-cover] img");
        await page.Locator("[data-blog-article]").First.ScrollIntoViewIfNeededAsync();
        await Expect(cover).ToBeVisibleAsync();
        await page.WaitForFunctionAsync("() => { const image = document.querySelector('[data-article-cover] img'); return image?.complete && image.naturalWidth >= 240; }");
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
        Assert.Empty(errors);
        var output = Environment.GetEnvironmentVariable("E2E_ILLUSTRATION_SCREENSHOT_DIR");
        if (!string.IsNullOrEmpty(output)) {
            Directory.CreateDirectory(output);
            await page.EvaluateAsync("window.scrollTo(0, 0)");
            await page.ScreenshotAsync(new() { Path = Path.Combine(output, $"editorial-{width}-{theme}.png") });
        }
        var destination = await first.Locator("a").First.GetAttributeAsync("href");
        await cover.ClickAsync();
        await Expect(page.Locator("[data-blog-article-detail]")).ToBeVisibleAsync();
        Assert.Contains(destination!.Split('?')[0], page.Url);
        await page.GotoAsync($"{baseUrl}/blog?bv=editorial&view=list");
        await Expect(page.Locator("[data-blog-article]").First).ToBeVisibleAsync();
        Assert.Equal(0, await page.Locator("[data-article-cover]").CountAsync());
    }

    [Theory]
    [MemberData(nameof(LayoutCases))]
    public async Task Collage_toggle_preserves_draft_survives_reload_and_stays_responsive(int width, string theme) {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = width, Height = 1300 } });
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);
        await page.AddInitScriptAsync($"localStorage.setItem('theme', '{theme}')");
        await page.GotoAsync($"{baseUrl}/blog?bv=editorial");
        var draft = page.GetByRole(AriaRole.Searchbox, new() { Name = "搜索文章", Exact = true });
        await draft.FillAsync("尚未提交的搜索");
        var toggle = page.GetByRole(AriaRole.Button, new() { Name = "拼图", Exact = true });
        await Expect(toggle).ToHaveAttributeAsync("aria-pressed", "true");
        await toggle.ClickAsync();
        await Expect(toggle).ToHaveAttributeAsync("aria-pressed", "true");
        await Expect(draft).ToHaveValueAsync("尚未提交的搜索");
        var cover = page.Locator("[data-blog-article]").First.Locator("[data-article-cover]");
        await Expect(cover).ToHaveAttributeAsync("data-cover-count", "3");
        Assert.DoesNotContain("search=", page.Url);
        await page.ReloadAsync();
        await Expect(toggle).ToHaveAttributeAsync("aria-pressed", "true");
        await Expect(cover).ToHaveAttributeAsync("data-cover-mode", "collage");
        await page.Locator("[data-blog-article]").First.ScrollIntoViewIfNeededAsync();
        await page.WaitForFunctionAsync("() => { const images = [...document.querySelector('[data-blog-article] [data-article-cover]').querySelectorAll('img')]; return images.length === 3 && images.every(image => image.complete && image.naturalWidth >= 96); }");
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
        var output = Environment.GetEnvironmentVariable("E2E_ILLUSTRATION_SCREENSHOT_DIR");
        if (!string.IsNullOrEmpty(output)) {
            Directory.CreateDirectory(output);
            await page.EvaluateAsync("window.scrollTo(0, 0)");
            await page.ScreenshotAsync(new() { Path = Path.Combine(output, $"collage-{width}-{theme}.png") });
        }
        await page.GetByRole(AriaRole.Button, new() { Name = "单图", Exact = true }).ClickAsync();
        await Expect(cover).ToHaveAttributeAsync("data-cover-mode", "single");
        await Expect(cover.Locator("img")).ToHaveCountAsync(1);
        await page.ReloadAsync();
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "单图", Exact = true })).ToHaveAttributeAsync("aria-pressed", "true");
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Collage_reflows_when_only_one_or_two_candidates_can_load(int count) {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = 320, Height = 1100 } });
        var response = await page.APIRequest.GetAsync($"{baseUrl}/api/v1/posts?pageSize=1");
        using var data = System.Text.Json.JsonDocument.Parse(await response.TextAsync());
        var blocked = data.RootElement.GetProperty("items")[0].GetProperty("coverImages").EnumerateArray()
            .Skip(count).Select(image => Uri.EscapeDataString(image.GetProperty("url").GetString()!)).ToArray();
        await page.RouteAsync("**/_next/image**", route => blocked.Any(url => route.Request.Url.Contains(url, StringComparison.OrdinalIgnoreCase)) ? route.AbortAsync() : route.ContinueAsync());
        await page.GotoAsync($"{baseUrl}/blog?bv=editorial");
        await page.GetByRole(AriaRole.Button, new() { Name = "拼图", Exact = true }).ClickAsync();
        var cover = page.Locator("[data-blog-article]").First.Locator("[data-article-cover]");
        await page.Locator("[data-blog-article]").First.ScrollIntoViewIfNeededAsync();
        await Expect(cover).ToHaveAttributeAsync("data-cover-count", count.ToString());
        await Expect(cover.Locator("img")).ToHaveCountAsync(count);
        await Expect(cover).ToHaveAttributeAsync("data-cover-mode", count == 1 ? "single" : "collage");
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
    }

    [Fact]
    public async Task Tiny_automatic_candidate_advances_to_the_next_real_image() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await browser.NewPageAsync();
        var response = await page.APIRequest.GetAsync($"{baseUrl}/api/v1/posts?pageSize=1");
        using var data = System.Text.Json.JsonDocument.Parse(await response.TextAsync());
        var firstUrl = data.RootElement.GetProperty("items")[0].GetProperty("coverImages")[0].GetProperty("url").GetString()!;
        var encoded = Uri.EscapeDataString(firstUrl);
        await page.RouteAsync("**/_next/image**", route => route.Request.Url.Contains(encoded, StringComparison.OrdinalIgnoreCase)
            ? route.FulfillAsync(new() { ContentType = "image/png", BodyBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jRZkAAAAASUVORK5CYII=") })
            : route.ContinueAsync());
        await page.AddInitScriptAsync("localStorage.setItem('starblog-cover-mode', 'single')");
        await page.GotoAsync($"{baseUrl}/blog?bv=editorial");
        var image = page.Locator("[data-blog-article]").First.Locator("[data-article-cover] img");
        await page.Locator("[data-blog-article]").First.ScrollIntoViewIfNeededAsync();
        await page.WaitForFunctionAsync("() => { const image = document.querySelector('[data-article-cover] img'); return image?.complete && image.naturalWidth >= 240; }");
        Assert.DoesNotContain(encoded, await image.GetAttributeAsync("src"));
    }

    [Fact]
    public async Task Failed_images_fall_back_to_text_and_other_layouts_are_unchanged() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await browser.NewPageAsync();
        await page.RouteAsync("**/_next/image**", route => route.AbortAsync());
        await page.GotoAsync($"{baseUrl}/blog?bv=editorial");
        var first = page.Locator("[data-blog-article]").First;
        await first.ScrollIntoViewIfNeededAsync();
        await Expect(first.Locator("[data-article-cover]")).ToHaveCountAsync(0, new() { Timeout = 30000 });
        await Expect(first.Locator("h2")).ToBeVisibleAsync();
        foreach (var variant in new[] { "library", "studio" }) {
            await page.GotoAsync($"{baseUrl}/blog?bv={variant}");
            await Expect(page.Locator("[data-blog-layout]")).ToBeVisibleAsync();
            Assert.Equal(0, await page.Locator("[data-article-cover]").CountAsync());
        }
    }
}
