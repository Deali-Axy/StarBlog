using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace StarBlog.E2ETests;

public sealed class BlogChromeTests {
    private static Task<IBrowser> LaunchAsync(IPlaywright playwright) => playwright.Chromium.LaunchAsync(new() {
        Headless = true,
        ExecutablePath = Environment.GetEnvironmentVariable("E2E_CHROMIUM_EXECUTABLE_PATH")
    });

    private static async Task SaveAsync(IPage page, string name) {
        var directory = Environment.GetEnvironmentVariable("E2E_BLOG_CHROME_SCREENSHOT_DIR");
        if (string.IsNullOrWhiteSpace(directory)) return;
        Directory.CreateDirectory(directory);
        await page.ScreenshotAsync(new() { Path = Path.Combine(directory, name + ".png") });
    }

    public static IEnumerable<object[]> ChromeCases() => BlogNavigationTests.DesignCases();

    [Theory]
    [MemberData(nameof(ChromeCases))]
    public async Task Chrome_matches_list_and_article_across_themes_and_viewports(string variant, int width, string theme) {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = width, Height = 1000 } });
        await E2EBaseUrl.UseApiOverrideAsync(page);
        await page.AddInitScriptAsync($"localStorage.setItem('theme', '{theme}')");
        await page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);
        await page.GotoAsync($"{baseUrl}/blog?bv={variant}");
        var chrome = page.Locator("[data-blog-chrome]");
        var header = page.GetByRole(AriaRole.Banner);
        var footer = page.GetByRole(AriaRole.Contentinfo);
        await Expect(chrome).ToHaveAttributeAsync("data-blog-chrome", variant);
        await Expect(header).ToHaveAttributeAsync("data-blog-header", variant);
        await Expect(footer).ToHaveAttributeAsync("data-blog-footer", variant);
        await Expect(header).ToHaveCountAsync(1);
        await Expect(footer).ToHaveCountAsync(1);
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
        var menu = header.GetByRole(AriaRole.Button, new() { Name = "站点导航", Exact = true });
        if (width < 1024 && variant != "editorial") {
            await menu.ClickAsync();
            var dialog = page.GetByRole(AriaRole.Dialog, new() { Name = "站点导航", Exact = true });
            await Expect(dialog).ToBeVisibleAsync();
            var menuCard = await dialog.EvaluateAsync<string>("el => getComputedStyle(el).getPropertyValue('--card').trim()");
            var shellCard = await chrome.EvaluateAsync<string>("el => getComputedStyle(el).getPropertyValue('--card').trim()");
            Assert.Equal(shellCard, menuCard);
            await page.Keyboard.PressAsync("Shift+Tab");
            Assert.True(await dialog.EvaluateAsync<bool>("el => el.contains(document.activeElement)"));
            await page.Keyboard.PressAsync("Escape");
            await Expect(dialog).ToBeHiddenAsync();
            await Expect(menu).ToBeFocusedAsync();
            await menu.ClickAsync();
            await dialog.GetByRole(AriaRole.Link, new() { Name = "文章", Exact = true }).ClickAsync();
            await Expect(dialog).ToBeHiddenAsync();
            Assert.Contains($"bv={variant}", page.Url);
        } else {
            var active = header.GetByRole(AriaRole.Link, new() { Name = "文章", Exact = true });
            await Expect(active).ToHaveAttributeAsync("aria-current", "page");
            await Expect(active).ToBeVisibleAsync();
        }
        await page.Locator("main h1").ClickAsync();
        await SaveAsync(page, $"chrome-{variant}-{width}-{theme}-header");
        var article = page.Locator("[data-blog-article]").First;
        var articleTitle = await article.GetByRole(AriaRole.Heading).InnerTextAsync();
        var articleHref = (await article.GetByRole(AriaRole.Link).GetAttributeAsync("href"))!;
        Assert.Contains($"bv={variant}", articleHref);
        await footer.ScrollIntoViewIfNeededAsync();
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
        await Expect(footer.GetByRole(AriaRole.Link, new() { Name = "文章", Exact = true })).ToHaveAttributeAsync("href", $"/blog?bv={variant}");
        await SaveAsync(page, $"chrome-{variant}-{width}-{theme}-footer");
        await footer.GetByRole(AriaRole.Button, new() { Name = "回到顶部", Exact = true }).ClickAsync();
        Assert.Equal(0, await page.EvaluateAsync<int>("Math.round(scrollY)"));
        await article.GetByRole(AriaRole.Link).ClickAsync();
        await Expect(page.Locator("[data-blog-article-detail]")).ToBeVisibleAsync();
        await Expect(page.Locator("main h1").First).ToHaveTextAsync(articleTitle);
        await Expect(chrome).ToHaveAttributeAsync("data-blog-chrome", variant);
        await Expect(header).ToHaveAttributeAsync("data-blog-header", variant);
        await Expect(footer).ToHaveAttributeAsync("data-blog-footer", variant);
        Assert.Contains($"bv={variant}", page.Url);
        Assert.DoesNotContain("bv=", (await page.Locator("link[rel=canonical]").GetAttributeAsync("href"))!);
        foreach (var link in await page.Locator("[data-blog-article-detail] a[href^='/blog/']").AllAsync()) {
            Assert.Contains($"bv={variant}", (await link.GetAttributeAsync("href"))!);
        }
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
        await SaveAsync(page, $"chrome-{variant}-{width}-{theme}-article");
        await page.ReloadAsync();
        await Expect(chrome).ToHaveAttributeAsync("data-blog-chrome", variant);
        await footer.GetByRole(AriaRole.Link, new() { Name = "文章", Exact = true }).ClickAsync();
        await Expect(page.Locator("[data-blog-variant]")).ToHaveAttributeAsync("data-blog-variant", variant);
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("library", "The Index")]
    [InlineData("editorial", "Field Notes")]
    [InlineData("studio", "The Archive")]
    public async Task Detail_style_switching_language_legacy_and_random_routes_preserve_the_layout(string variant, string englishTitle) {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = 375, Height = 1000 } });
        await E2EBaseUrl.UseApiOverrideAsync(page);
        await page.GotoAsync($"{baseUrl}/blog?bv={variant}");
        var href = (await page.Locator("[data-blog-article] a").First.GetAttributeAsync("href"))!;
        var slug = new Uri(new Uri(baseUrl), href).AbsolutePath.Split('/').Last();
        await page.GotoAsync($"{baseUrl}/p/{slug}?lang=original&bv={variant}");
        await Expect(page.Locator("[data-blog-article-detail]")).ToBeVisibleAsync();
        Assert.Contains($"bv={variant}", page.Url);
        Assert.Contains("lang=original", page.Url);
        var originalUrl = page.Url;
        var next = variant == "studio" ? "library" : "studio";
        var nextLabel = next == "studio" ? "档案工作台" : "活字索引";
        await page.GetByRole(AriaRole.Button, new() { Name = nextLabel, Exact = true }).ClickAsync();
        await Expect(page.Locator("[data-blog-chrome]")).ToHaveAttributeAsync("data-blog-chrome", next);
        Assert.Equal(new Uri(originalUrl).AbsolutePath, new Uri(page.Url).AbsolutePath);
        Assert.Contains("lang=original", page.Url);
        await page.GoBackAsync();
        await Expect(page.Locator("[data-blog-chrome]")).ToHaveAttributeAsync("data-blog-chrome", variant);
        await page.GetByRole(AriaRole.Banner).GetByRole(AriaRole.Button, new() { Name = "Switch to English", Exact = true }).ClickAsync();
        await Expect(page.Locator("html")).ToHaveAttributeAsync("lang", "en");
        Assert.Contains($"bv={variant}", page.Url);
        Assert.DoesNotContain("lang=", page.Url);
        await Expect(page.GetByRole(AriaRole.Group, new() { Name = "Design preview", Exact = true })).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Contentinfo).GetByRole(AriaRole.Link, new() { Name = "Articles", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = englishTitle, Exact = true })).ToBeVisibleAsync();
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
        await page.GotoAsync($"{baseUrl}/blog/random?bv={variant}");
        await Expect(page.Locator("[data-blog-article-detail]")).ToBeVisibleAsync();
        Assert.Contains($"bv={variant}", page.Url);
        await Expect(page.Locator("[data-blog-chrome]")).ToHaveAttributeAsync("data-blog-chrome", variant);
    }
}
