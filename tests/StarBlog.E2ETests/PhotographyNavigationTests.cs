using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace StarBlog.E2ETests;

public sealed partial class PhotographyNavigationTests {
    private static Task<IBrowser> LaunchAsync(IPlaywright playwright) => playwright.Chromium.LaunchAsync(new() {
        Headless = true,
        ExecutablePath = Environment.GetEnvironmentVariable("E2E_CHROMIUM_EXECUTABLE_PATH")
    });

    private static async Task<IPage> NewPageAsync(IBrowser browser, int width = 1280, int height = 900) {
        var page = await browser.NewPageAsync(new() {
            ViewportSize = new() { Width = width, Height = height },
            HasTouch = width < 640,
            IsMobile = width < 640
        });
        await E2EBaseUrl.UseApiOverrideAsync(page);
        return page;
    }

    private static async Task SelectDensityAsync(IPage page, string label) {
        await page.GetByRole(AriaRole.Combobox, new() { Name = "照片密度", Exact = true }).ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = label, Exact = true }).ClickAsync();
    }

    [Theory]
    [InlineData(375, "light")]
    [InlineData(375, "dark")]
    [InlineData(768, "light")]
    [InlineData(768, "dark")]
    [InlineData(1280, "light")]
    [InlineData(1280, "dark")]
    [InlineData(1920, "light")]
    [InlineData(1920, "dark")]
    public async Task Neutral_theme_and_responsive_shell_follow_site_preference(int width, string theme) {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser, width);
        await page.AddInitScriptAsync($"localStorage.setItem('theme', '{theme}')");
        var errors = new List<string>();
        page.PageError += (_, message) => errors.Add(message);
        await page.GotoAsync($"{baseUrl}/photography");
        await Expect(page.Locator("[data-photo-id]").First).ToBeVisibleAsync();
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
        var expected = theme == "light" ? "rgb(255, 255, 255)" : "rgb(8, 8, 8)";
        Assert.Equal(expected, await page.Locator(".photography-theme").First.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor"));
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "搜索文章", Exact = true })).ToBeVisibleAsync();
        if (width < 640) {
            await Expect(page.Locator(".photo-touch-caption").First).ToHaveCSSAsync("opacity", "1");
            await page.GetByRole(AriaRole.Button, new() { Name = "站点导航", Exact = true }).ClickAsync();
            await Expect(page.GetByRole(AriaRole.Menu)).ToBeVisibleAsync();
            await page.Keyboard.PressAsync("Escape");
        }
        if (width >= 1024) await Expect(page.GetByRole(AriaRole.Button, new() { Name = "站点导航", Exact = true })).ToBeHiddenAsync();
        var density = page.GetByRole(AriaRole.Combobox, new() { Name = "照片密度", Exact = true });
        await density.ClickAsync();
        var options = page.GetByRole(AriaRole.Listbox);
        await Expect(options).ToBeVisibleAsync();
        await Expect(options).ToHaveCSSAsync("background-color", theme == "light" ? "rgb(255, 255, 255)" : "rgb(20, 20, 20)");
        await Expect(page.GetByRole(AriaRole.Option, new() { Name = "标准", Exact = true })).ToHaveAttributeAsync("aria-selected", "true");
        await Expect(page.GetByRole(AriaRole.Option, new() { Name = "标准", Exact = true })).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("End");
        await Expect(page.GetByRole(AriaRole.Option, new() { Name = "紧凑", Exact = true })).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Enter");
        await Expect(page.Locator("[data-photo-wall]")).ToHaveAttributeAsync("data-density", "compact");
        await Expect(density).ToBeFocusedAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "切换昼夜", Exact = true }).ClickAsync();
        var next = theme == "light" ? "rgb(8, 8, 8)" : "rgb(255, 255, 255)";
        await Expect(page.Locator(".photography-theme").First).ToHaveCSSAsync("background-color", next);
        await page.GotoAsync($"{baseUrl}/about");
        await Expect(page.Locator("header .page-wrap")).ToBeVisibleAsync();
        await Expect(page.Locator(".photography-theme")).ToHaveCountAsync(0);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task Layout_and_density_persist_and_preserve_the_visible_photo() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser);
        await page.GotoAsync($"{baseUrl}/photography");
        var wall = page.Locator("[data-photo-wall]");
        await Expect(wall).ToHaveAttributeAsync("data-view", "masonry");
        await Expect(wall).ToHaveAttributeAsync("data-density", "standard");
        await page.EvaluateAsync("scrollTo(0, 600)");
        var anchor = await page.EvaluateAsync<string>("() => { const bottom = document.querySelector('[data-photo-toolbar]').getBoundingClientRect().bottom; return [...document.querySelectorAll('[data-photo-id]')].find(el => el.getBoundingClientRect().bottom > bottom).id }");
        var top = await page.Locator($"#{anchor}").EvaluateAsync<double>("el => el.getBoundingClientRect().top");
        await page.GetByRole(AriaRole.Button, new() { Name = "齐行", Exact = true }).ClickAsync();
        await Expect(wall).ToHaveAttributeAsync("data-view", "justified");
        Assert.InRange(Math.Abs(await page.Locator($"#{anchor}").EvaluateAsync<double>("el => el.getBoundingClientRect().top") - top), 0, 2);
        await SelectDensityAsync(page, "紧凑");
        await page.ReloadAsync();
        await Expect(wall).ToHaveAttributeAsync("data-view", "justified");
        await Expect(wall).ToHaveAttributeAsync("data-density", "compact");
    }

    [Fact]
    public async Task Detail_and_nested_lightbox_isolate_background_and_restore_focus() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser);
        var originals = 0;
        page.Request += (_, request) => { if (new Uri(request.Url).AbsolutePath.StartsWith("/media/photography/")) originals++; };
        await page.GotoAsync($"{baseUrl}/photography");
        var link = page.Locator("[data-photo-id] a").Nth(8);
        await link.ScrollIntoViewIfNeededAsync();
        var triggerId = await link.GetAttributeAsync("id");
        var scrollY = await page.EvaluateAsync<double>("scrollY");
        await link.ClickAsync();
        await Expect(page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
        await Expect(page.Locator("header")).ToHaveAttributeAsync("aria-hidden", "true");
        await Expect(page.Locator("footer")).ToHaveAttributeAsync("aria-hidden", "true");
        await Expect(page.GetByText("上传时间", new() { Exact = true })).ToBeVisibleAsync();
        Assert.Equal(0, originals);
        await page.GetByRole(AriaRole.Button, new() { Name = "查看大图", Exact = true }).ClickAsync();
        await Expect(page.Locator("[role=dialog]")).ToHaveCountAsync(2);
        await page.WaitForFunctionAsync("document.querySelectorAll('[role=dialog] img').length >= 2");
        var detailUrl = page.Url;
        await page.Keyboard.PressAsync("ArrowRight");
        await page.WaitForURLAsync(url => url != detailUrl);
        await Expect(page.Locator("[role=dialog]")).ToHaveCountAsync(2);
        await page.Keyboard.PressAsync("Escape");
        await Expect(page.Locator("[role=dialog]")).ToHaveCountAsync(1);
        await page.Keyboard.PressAsync("Escape");
        await page.WaitForURLAsync($"{baseUrl}/photography");
        await Expect(page.Locator($"#{triggerId}")).ToBeFocusedAsync();
        Assert.InRange(Math.Abs(await page.EvaluateAsync<double>("scrollY") - scrollY), 0, 2);
        Assert.True(originals > 0);
        await Expect(page.Locator("header")).Not.ToHaveAttributeAsync("aria-hidden", "true");
    }

    [Fact]
    public async Task Direct_detail_on_a_small_screen_scrolls_and_closes_to_wall() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser, 375, 500);
        await page.GotoAsync($"{baseUrl}/photography");
        var href = await page.Locator("[data-photo-id] a").First.GetAttributeAsync("href");
        await page.GotoAsync($"{baseUrl}{href}");
        var dialog = page.GetByRole(AriaRole.Dialog);
        await Expect(dialog).ToBeVisibleAsync();
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
        await page.GetByText("上传时间", new() { Exact = true }).ScrollIntoViewIfNeededAsync();
        await Expect(page.GetByText("上传时间", new() { Exact = true })).ToBeInViewportAsync();
        await page.Keyboard.PressAsync("Escape");
        await page.WaitForURLAsync($"{baseUrl}/photography");
        await Expect(page.Locator("[role=dialog]")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Pagination_is_manual_and_failed_requests_can_be_retried() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser);
        await page.GotoAsync($"{baseUrl}/photography");
        var tiles = page.Locator("[data-photo-id]");
        await Expect(tiles.First).ToBeVisibleAsync();
        var before = await tiles.CountAsync();
        var more = page.GetByRole(AriaRole.Button, new() { Name = "加载更多", Exact = true });
        if (await more.CountAsync() == 0) {
            await Expect(page.GetByText("已展示全部照片", new() { Exact = true })).ToBeVisibleAsync();
            return;
        }
        await page.Locator("footer").ScrollIntoViewIfNeededAsync();
        await Expect(page.Locator("footer")).ToBeInViewportAsync();
        Assert.Equal(before, await tiles.CountAsync());
        var fail = true;
        await page.RouteAsync("**/api/v1/photos?*", async route => {
            if (fail) { fail = false; await route.AbortAsync(); }
            else await route.FallbackAsync();
        });
        await more.ClickAsync();
        await Expect(page.Locator("p[role=alert]")).ToContainTextAsync("照片列表加载失败");
        await page.GetByRole(AriaRole.Button, new() { Name = "重试", Exact = true }).ClickAsync();
        await page.WaitForFunctionAsync("before => document.querySelectorAll('[data-photo-id]').length > before", before);
        var ids = await tiles.EvaluateAllAsync<string[]>("els => els.map(el => el.dataset.photoId)");
        Assert.Equal(ids.Length, ids.Distinct().Count());
    }

    [Fact]
    public async Task Failed_images_and_random_navigation_offer_feedback_and_retry() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser);
        string? failedPath = null;
        var failImage = true;
        await page.RouteAsync("**/api/v1/photos/*/thumbnail?*", async route => {
            var path = new Uri(route.Request.Url).AbsolutePath;
            failedPath ??= path;
            if (failImage && path == failedPath) await route.AbortAsync();
            else await route.FallbackAsync();
        });
        await page.GotoAsync($"{baseUrl}/photography");
        await Expect(page.GetByText("照片加载失败", new() { Exact = true })).ToBeVisibleAsync();
        failImage = false;
        await page.GetByRole(AriaRole.Button, new() { NameRegex = new("^重试:") }).ClickAsync();
        await Expect(page.GetByText("照片加载失败", new() { Exact = true })).ToHaveCountAsync(0);
        await page.RouteAsync("**/api/v1/site/photos/random", route => route.FulfillAsync(new() { Status = 503, ContentType = "application/json", Body = "{}" }));
        await page.GetByRole(AriaRole.Button, new() { Name = "随机一张", Exact = true }).ClickAsync();
        await Expect(page.GetByText("暂时无法打开随机照片，请重试。", new() { Exact = true })).ToBeVisibleAsync();
        Assert.EndsWith("/photography", page.Url);
    }

    [Fact]
    public async Task Preferences_work_when_browser_storage_is_blocked() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser);
        await page.AddInitScriptAsync("Object.defineProperty(window, 'localStorage', { get() { throw new DOMException('Blocked', 'SecurityError') } })");
        await page.GotoAsync($"{baseUrl}/photography");
        await page.GetByRole(AriaRole.Button, new() { Name = "齐行", Exact = true }).ClickAsync();
        await SelectDensityAsync(page, "宽松");
        await Expect(page.Locator("[data-photo-wall]")).ToHaveAttributeAsync("data-view", "justified");
        await Expect(page.Locator("[data-photo-wall]")).ToHaveAttributeAsync("data-density", "comfortable");
    }

    [Fact]
    public async Task Failed_clipboard_sharing_reports_the_failure() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser);
        await page.AddInitScriptAsync("Object.defineProperty(navigator, 'share', { value: undefined }); Object.defineProperty(navigator, 'clipboard', { value: { writeText() { return Promise.reject(new Error('Unavailable')) } } })");
        await page.GotoAsync($"{baseUrl}/photography");
        await page.Locator("[data-photo-id] a").First.ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "分享", Exact = true }).ClickAsync();
        await Expect(page.GetByText("分享失败，请重试。", new() { Exact = true })).ToBeVisibleAsync();
    }
}
