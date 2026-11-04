using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace StarBlog.E2ETests;

public sealed class SeoTests {
    private static Task<IBrowser> LaunchAsync(IPlaywright playwright) => playwright.Chromium.LaunchAsync(new() {
        Headless = true, ExecutablePath = Environment.GetEnvironmentVariable("E2E_CHROMIUM_EXECUTABLE_PATH")
    });

    [Theory]
    [InlineData("/", "/")]
    [InlineData("/blog?bv=studio&view=list", "/blog")]
    [InlineData("/blog?page=2&pageSize=6&view=list", "/blog")]
    [InlineData("/photography", "/photography")]
    [InlineData("/photography/catalog?page=2", "/photography/catalog")]
    [InlineData("/about?uv=editorial", "/about")]
    [InlineData("/links?uv=editorial", "/links")]
    [InlineData("/rss", "/rss")]
    [InlineData("/search?keyword=React", "/search")]
    public async Task Pages_emit_independent_metadata_in_crawler_HTML(string path, string canonicalPath) {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await browser.NewPageAsync(new() { JavaScriptEnabled = false, UserAgent = "Twitterbot" });
        var response = await page.GotoAsync(baseUrl + path);
        Assert.Equal(200, response!.Status);
        var title = await page.TitleAsync();
        Assert.Contains("StarBlog", title);
        Assert.NotEqual("首页", title);
        Assert.NotEqual("主页", title);
        Assert.Single(await page.Locator("head title").AllAsync());
        var canonical = new Uri((await page.Locator("head link[rel=canonical]").GetAttributeAsync("href"))!);
        Assert.Equal(canonicalPath, canonical.AbsolutePath);
        Assert.DoesNotContain("bv=", canonical.Query);
        Assert.DoesNotContain("view=", canonical.Query);
        if (path.Contains("page=2")) Assert.Contains("page=2", canonical.Query);
        await Expect(page.Locator("head meta[property='og:title']")).ToHaveAttributeAsync("content", title);
        await Expect(page.Locator("head meta[name='twitter:title']")).ToHaveAttributeAsync("content", title);
        Assert.Equal(canonical, new Uri((await page.Locator("head meta[property='og:url']").GetAttributeAsync("content"))!));
        Assert.False(string.IsNullOrWhiteSpace(await page.Locator("head meta[name=description]").GetAttributeAsync("content")));
        var image = (await page.Locator("head meta[property='og:image']").First.GetAttributeAsync("content"))!;
        Assert.True(Uri.TryCreate(image, UriKind.Absolute, out _));
        if (path.StartsWith("/search")) Assert.Contains("noindex", await page.Locator("meta[name=robots]").First.GetAttributeAsync("content"));
    }

    [Fact]
    public async Task Articles_have_reciprocal_translations_and_matching_structured_data() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await browser.NewPageAsync(new() { JavaScriptEnabled = false, UserAgent = "Twitterbot" });
        foreach (var language in new[] { "original", "en", "does-not-exist" }) {
            await page.GotoAsync($"{baseUrl}/blog/article-reading-fixture?lang={language}&bv=studio");
            var canonical = (await page.Locator("link[rel=canonical]").GetAttributeAsync("href"))!;
            Assert.EndsWith(language == "en" ? "/blog/article-reading-fixture?lang=en" : "/blog/article-reading-fixture", canonical);
            await Expect(page.Locator("link[hreflang=en]")).ToHaveAttributeAsync("href", new System.Text.RegularExpressions.Regex(@"/blog/article-reading-fixture\?lang=en$"));
            await Expect(page.Locator("link[hreflang='zh-CN']")).ToHaveAttributeAsync("href", new System.Text.RegularExpressions.Regex("/blog/article-reading-fixture$"));
            using var json = JsonDocument.Parse(await page.Locator("script[type='application/ld+json']").First.TextContentAsync() ?? "{}");
            var article = json.RootElement.GetProperty("@graph")[0];
            Assert.Equal(canonical, article.GetProperty("url").GetString());
            Assert.Equal(canonical, article.GetProperty("mainEntityOfPage").GetString());
            Assert.Equal(language == "en" ? "en" : "zh-CN", article.GetProperty("inLanguage").GetString());
            if (language == "en") Assert.Contains("Making complex ideas clear", await page.TitleAsync());
        }
    }

    [Theory]
    [InlineData("/this-page-does-not-exist")]
    [InlineData("/blog/this-post-does-not-exist")]
    [InlineData("/blog/100%25")]
    [InlineData("/photography/this-photo-does-not-exist")]
    [InlineData("/photography/catalog?page=999999")]
    public async Task Missing_pages_are_not_indexable(string path) {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await browser.NewPageAsync(new() { UserAgent = "Twitterbot" });
        var response = await page.GotoAsync(baseUrl + path);
        // Next may already have streamed the shell when a nested page calls notFound.
        Assert.True(response!.Status is 404 or 200);
        Assert.Contains("noindex", string.Join(",", await page.Locator("meta[name=robots]").EvaluateAllAsync<string[]>("nodes => nodes.map(n => n.content)")));
        await Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 }).First).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Photo_catalog_and_detail_are_readable_without_JavaScript() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await browser.NewPageAsync(new() { JavaScriptEnabled = false, UserAgent = "Twitterbot" });
        await page.GotoAsync(baseUrl + "/photography/catalog?page=2");
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "摄影目录", Exact = true })).ToBeVisibleAsync();
        var photos = page.Locator("section ul a[href^='/photography/']");
        Assert.True(await photos.CountAsync() > 0);
        var href = await photos.First.GetAttributeAsync("href");
        await page.GotoAsync(baseUrl + href);
        await Expect(page.Locator("article h1")).ToBeVisibleAsync();
        Assert.True(await page.Locator("article img[alt]").CountAsync() > 0);
        Assert.True(await page.Locator("script[type='application/ld+json']").CountAsync() > 0);
    }

    [Theory]
    [InlineData(375)]
    [InlineData(1280)]
    public async Task Photo_catalog_navigation_and_dialog_work_after_hydration(int width) {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = width, Height = 900 } });
        await page.GotoAsync(baseUrl + "/photography");
        await page.GetByRole(AriaRole.Link, new() { Name = "浏览全部照片目录", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "摄影目录", Exact = true })).ToBeVisibleAsync();
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
        await page.GetByRole(AriaRole.Link, new() { Name = "下一页", Exact = true }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/photography/catalog\?page=2$"));
        var first = page.Locator("section ul a[href^='/photography/']").First;
        await first.ClickAsync();
        await Expect(page.GetByRole(AriaRole.Dialog).First).ToBeVisibleAsync();
        Assert.Contains("StarBlog", await page.TitleAsync());
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
        var screenshotDir = Environment.GetEnvironmentVariable("E2E_SEO_SCREENSHOT_DIR");
        if (!string.IsNullOrWhiteSpace(screenshotDir)) {
            Directory.CreateDirectory(screenshotDir);
            await page.ScreenshotAsync(new() { Path = Path.Combine(screenshotDir, $"photo-detail-{width}.png") });
        }
    }

    [Fact]
    public async Task Sitemap_indexes_and_chunks_use_public_canonical_addresses() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var client = new HttpClient();
        var robots = await client.GetStringAsync(baseUrl + "/robots.txt");
        Assert.Contains("Sitemap:", robots);
        var index = XDocument.Parse(await client.GetStringAsync(baseUrl + "/sitemap.xml"));
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var locations = index.Descendants(ns + "loc").Select(x => new Uri(x.Value)).ToList();
        Assert.True(locations.Count >= 3);
        foreach (var location in locations) {
            Assert.DoesNotContain("127.0.0.1", location.Host);
            var xml = XDocument.Parse(await client.GetStringAsync(baseUrl + location.PathAndQuery));
            var urls = xml.Descendants(ns + "loc").Select(x => new Uri(x.Value)).ToList();
            Assert.NotEmpty(urls);
            Assert.All(urls, url => Assert.Equal(location.Host, url.Host));
            Assert.True(urls.Count <= 1000);
            Assert.All(urls, url => Assert.DoesNotContain("bv=", url.Query));
        }
        using var invalid = await client.GetAsync(baseUrl + "/sitemaps/posts/999999.xml");
        Assert.Equal(404, (int)invalid.StatusCode);
        using var redirects = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
        using var legacy = await redirects.GetAsync(baseUrl + "/p/article-reading-fixture?lang=en");
        Assert.Equal(308, (int)legacy.StatusCode);
        Assert.Contains("lang=en", legacy.Headers.Location!.ToString());
        using var cover = await client.GetAsync(baseUrl + "/og-default.png");
        Assert.Equal("image/png", cover.Content.Headers.ContentType!.MediaType);
        Assert.True((await cover.Content.ReadAsByteArrayAsync()).Length > 1000);
    }
}
