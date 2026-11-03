using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace StarBlog.E2ETests;

public sealed class HomeCustomizationTests {
    private static string DesignUrl(string baseUrl, string preset = "editorial", object? overrides = null) {
        var json = JsonSerializer.Serialize(new { version = 2, preset, overrides = overrides ?? new { } });
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return $"{baseUrl}/?hc=v2.{encoded}&keep=C%23#home-top";
    }

    private sealed class Session : IAsyncDisposable {
        public required IPlaywright Playwright { get; init; }
        public required IBrowser Browser { get; init; }
        public required IPage Page { get; init; }
        public static async Task<Session> Start(int width = 1440, string mode = "light") {
            var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new() {
                Headless = true, ExecutablePath = Environment.GetEnvironmentVariable("E2E_CHROMIUM_EXECUTABLE_PATH")
            });
            var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = width, Height = 1000 }, ReducedMotion = ReducedMotion.Reduce });

            page.SetDefaultNavigationTimeout(60000);
            await page.AddInitScriptAsync($"if (!localStorage.getItem('theme')) localStorage.setItem('theme', '{mode}')");
            return new Session { Playwright = playwright, Browser = browser, Page = page };
        }
        public async ValueTask DisposeAsync() { await Browser.DisposeAsync(); Playwright.Dispose(); }
    }
    private static ILocator Panel(IPage page) => page.Locator("[data-home-design-menu]");
    private static async Task Open(IPage page) {
        await page.Locator("[data-home-design-trigger]").ClickAsync();
        await Expect(Panel(page)).ToBeVisibleAsync();
    }
    private static async Task Choose(IPage page, string path, string value) {
        await Panel(page).Locator($"[data-home-setting='{path}'] input[value='{value}']").CheckAsync();
    }

    [Fact]
    public async Task Draft_cancel_undo_apply_and_history_preserve_article_state() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        await using var session = await Session.Start();
        var page = session.Page;
        await page.GotoAsync($"{baseUrl}/?keep=C%23#home-top", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        var preview = page.Locator("[data-home-section='latestPosts'] button[aria-expanded]").First;
        await preview.ClickAsync();
        var expanded = page.Locator("[data-home-row][data-expanded='true']");
        await Expect(expanded).ToHaveCountAsync(1);
        var original = page.Url;
        await Open(page);
        await Panel(page).GetByRole(AriaRole.Tab, new() { Name = "内容模块", Exact = true }).ClickAsync();
        await Choose(page, "sections.photos", "contactSheet");
        await Expect(page.Locator("[data-home-section='photos']")).ToHaveAttributeAsync("data-home-variant", "contactSheet");
        Assert.Equal(original, page.Url);
        Assert.DoesNotContain(await page.Context.CookiesAsync(), cookie => cookie.Name == "starblog.home-design");
        await page.Keyboard.PressAsync("Escape");
        await Expect(Panel(page)).ToHaveCountAsync(0);
        await Expect(page.Locator("[data-home-design-trigger]")).ToBeFocusedAsync();
        await Expect(page.Locator("[data-home-section='photos']")).ToHaveAttributeAsync("data-home-variant", "prints");
        await Open(page);
        await Panel(page).GetByRole(AriaRole.Tab, new() { Name = "页面布局", Exact = true }).ClickAsync();
        await Choose(page, "navigation", "compact");
        await Expect(page.Locator("[data-home-layout]")).ToHaveAttributeAsync("data-home-layout", "topbar");
        await Expect(Panel(page).GetByText("已同步调整框架与导航，确保布局匹配。")).ToBeVisibleAsync();
        await Panel(page).GetByRole(AriaRole.Button, new() { Name = "撤销本次修改", Exact = true }).ClickAsync();
        await Expect(page.Locator("[data-home-layout]")).ToHaveAttributeAsync("data-home-layout", "sidebar");
        await Choose(page, "navigation", "compact");
        await Panel(page).GetByRole(AriaRole.Button, new() { Name = "应用", Exact = true }).ClickAsync();
        await Expect(Panel(page)).ToHaveCountAsync(0);
        Assert.Contains("hc=v2.", page.Url);
        Assert.Contains("keep=C%23", page.Url);
        Assert.Contains("#home-top", page.Url);
        Assert.Contains(await page.Context.CookiesAsync(), cookie => cookie.Name == "starblog.home-design" && cookie.Value.StartsWith("v2."));
        await Expect(expanded).ToHaveCountAsync(1);
        await page.EvaluateAsync("history.back()");
        await Expect(page.Locator("[data-home-layout]")).ToHaveAttributeAsync("data-home-layout", "sidebar");
        await page.EvaluateAsync("history.forward()");
        await Expect(page.Locator("[data-home-layout]")).ToHaveAttributeAsync("data-home-layout", "topbar");
        await Expect(expanded).ToHaveCountAsync(1);
        await page.ReloadAsync(new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Expect(page.Locator("[data-home-layout]")).ToHaveAttributeAsync("data-home-layout", "topbar");
        await page.GotoAsync($"{baseUrl}/about", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Expect(page.Locator("[data-home-design]")).ToHaveCountAsync(0);
        await page.GotoAsync($"{baseUrl}/", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Expect(page.Locator("[data-home-layout]")).ToHaveAttributeAsync("data-home-layout", "topbar");
        await page.GetByRole(AriaRole.Button, new() { Name = "Switch to English", Exact = true }).ClickAsync();
        await Expect(page.Locator("html")).ToHaveAttributeAsync("lang", "en");
        await Expect(page.Locator("[data-home-layout]")).ToHaveAttributeAsync("data-home-layout", "topbar");
    }

    [Fact]
    public async Task Theme_preview_is_scoped_until_apply_and_custom_colors_are_independent() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        await using var session = await Session.Start();
        var page = session.Page;
        await page.GotoAsync($"{baseUrl}/", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Open(page);
        await Panel(page).GetByRole(AriaRole.Tab, new() { Name = "外观", Exact = true }).ClickAsync();
        var originalFont = await page.Locator("h1").EvaluateAsync<string>("el => getComputedStyle(el).fontFamily");
        await Choose(page, "theme.typography", "modern");
        Assert.NotEqual(originalFont, await page.Locator("h1").EvaluateAsync<string>("el => getComputedStyle(el).fontFamily"));
        await Panel(page).GetByRole(AriaRole.Button, new() { Name = "字体组合: 恢复默认", Exact = true }).ClickAsync();
        Assert.Equal(originalFont, await page.Locator("h1").EvaluateAsync<string>("el => getComputedStyle(el).fontFamily"));
        await Choose(page, "theme.palette", "forest");
        await Panel(page).Locator("input[name='home-mode']").Nth(2).CheckAsync();
        await Expect(page.Locator("[data-home-design]")).ToHaveAttributeAsync("data-home-mode", "dark");
        Assert.False(await page.Locator("html").EvaluateAsync<bool>("el => el.classList.contains('dark')"));
        Assert.Equal("light", await page.EvaluateAsync<string>("localStorage.getItem('theme')"));
        var colors = await page.EvaluateAsync<string[]>("() => [document.querySelector('[data-home-design]'),document.querySelector('[data-home-design-menu]')].map(el => getComputedStyle(el).getPropertyValue('--background').trim())");
        Assert.Equal(colors[0], colors[1]);
        Assert.Equal("#16221c", colors[0]);
        await Panel(page).Locator("summary").ClickAsync();
        var darkButton = Panel(page).GetByRole(AriaRole.Group, new() { Name = "高级调色", Exact = true }).GetByRole(AriaRole.Button, new() { Name = "深色", Exact = true });
        await darkButton.ClickAsync();
        var input = Panel(page).Locator("[data-home-setting='theme.colors.dark.primary'] input[type='text']");
        await input.FillAsync("#abcdef");
        await input.PressAsync("Enter");
        await Panel(page).GetByRole(AriaRole.Group, new() { Name = "高级调色", Exact = true }).GetByRole(AriaRole.Button, new() { Name = "浅色", Exact = true }).ClickAsync();
        await Expect(Panel(page).Locator("[data-home-setting='theme.colors.light.primary'] input[type='text']")).ToHaveValueAsync("#3b684d");
        await Panel(page).GetByRole(AriaRole.Button, new() { Name = "取消", Exact = true }).ClickAsync();
        Assert.Equal("light", await page.EvaluateAsync<string>("localStorage.getItem('theme')"));
        await Open(page);
        await Choose(page, "theme.palette", "forest");
        await Panel(page).Locator("input[name='home-mode']").Nth(2).CheckAsync();
        await Panel(page).GetByRole(AriaRole.Button, new() { Name = "应用", Exact = true }).ClickAsync();
        await Expect(page.Locator("html")).ToHaveClassAsync(new Regex("dark"));
        Assert.Equal("dark", await page.EvaluateAsync<string>("localStorage.getItem('theme')"));
        await Open(page);
        await Panel(page).Locator("input[name='home-mode'][value='system']").CheckAsync();
        await page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        await Expect(page.Locator("[data-home-design]")).ToHaveAttributeAsync("data-home-mode", "dark");
        await Panel(page).GetByRole(AriaRole.Button, new() { Name = "应用", Exact = true }).ClickAsync();
        Assert.Equal("system", await page.EvaluateAsync<string>("localStorage.getItem('theme')"));
        await page.GotoAsync($"{baseUrl}/photography", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Expect(page.Locator("[data-home-design]")).ToHaveCountAsync(0);
        Assert.True(await page.Locator("html").EvaluateAsync<bool>("el => el.classList.contains('dark')"));
    }

    [Fact]
    public async Task Shared_configuration_ignores_saved_design_preserves_mode_and_can_be_saved() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        await using var session = await Session.Start();
        var page = session.Page;
        await page.Context.AddCookiesAsync([new Cookie { Name = "starblog.home-design", Value = "desk.tilt", Url = baseUrl }]);
        await page.AddInitScriptAsync("Object.defineProperty(navigator, 'clipboard', {value: {writeText: () => Promise.reject(new Error('Unavailable'))}})");
        await page.GotoAsync(DesignUrl(baseUrl, overrides: new { theme = new { palette = "forest" }, sections = new { photos = "prints" } }), new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Expect(page.Locator("[data-home-design]")).ToHaveAttributeAsync("data-home-design", "editorial");
        await Expect(page.Locator("[data-home-categories]")).ToHaveAttributeAsync("data-home-categories", "accordion");
        Assert.Equal("light", await page.EvaluateAsync<string>("localStorage.getItem('theme')"));
        await Open(page);
        await Panel(page).GetByRole(AriaRole.Button, new() { Name = "分享当前配置", Exact = true }).ClickAsync();
        var link = await Panel(page).GetByRole(AriaRole.Textbox, new() { Name = "分享当前配置", Exact = true }).InputValueAsync();
        Assert.Contains("hc=v2.", link);
        Assert.Contains("keep=C%23", link);
        Assert.DoesNotContain("mode", link);
        Assert.Equal("desk.tilt", (await page.Context.CookiesAsync()).Single(cookie => cookie.Name == "starblog.home-design").Value);
        await Panel(page).GetByRole(AriaRole.Button, new() { Name = "应用", Exact = true }).ClickAsync();
        await page.GotoAsync($"{baseUrl}/", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Expect(page.Locator("[data-home-section='photos']")).ToHaveAttributeAsync("data-home-variant", "prints");
    }

    [Theory]
    [InlineData(375, "light")]
    [InlineData(375, "dark")]
    [InlineData(768, "light")]
    [InlineData(768, "dark")]
    [InlineData(1440, "light")]
    [InlineData(1440, "dark")]
    public async Task Presets_heroes_palettes_and_panel_fit_the_viewport(int width, string mode) {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        await using var session = await Session.Start(width, mode);
        var page = session.Page;
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);
        var index = 0;
        foreach (var preset in new[] { "journal", "editorial", "desk" }) {
            foreach (var hero in new[] { "cover", "signature", "photograph" }) {
                var palette = new[] { "paper", "slate", "forest", "ink" }[index++ % 4];
                await page.GotoAsync(DesignUrl(baseUrl, preset, new { hero, theme = new { palette, fontScale = 115, density = "compact" }, contentWidth = "narrow" }), new() { WaitUntil = WaitUntilState.DOMContentLoaded });
                await Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveCountAsync(1);
                Assert.True(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth + 1"), $"Overflow: {preset}/{hero}/{width}/{mode}");
                await Open(page);
                var box = await Panel(page).BoundingBoxAsync();
                Assert.NotNull(box);
                Assert.True(box.X >= 0 && box.X + box.Width <= width + 1);
                Assert.True(box.Y >= 0 && box.Y + box.Height <= 1001);
                if (Environment.GetEnvironmentVariable("E2E_HOME_SCREENSHOT_DIR") is { Length: > 0 } directory) {
                    Directory.CreateDirectory(directory);
                    await page.ScreenshotAsync(new() { Path = Path.Combine(directory, $"{width}-{mode}-{preset}-{hero}.png") });
                }
                await page.Keyboard.PressAsync("Escape");
            }
        }
        Assert.Empty(errors);
    }

    [Fact]
    public async Task Photography_failure_falls_back_without_duplicate_heading() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        await using var session = await Session.Start();
        var page = session.Page;
        await page.GotoAsync(DesignUrl(baseUrl, overrides: new { hero = "photograph" }), new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        // Wait until the synthetic event can reach the hydrated image handler.
        await Expect(page.Locator("[data-home-design-trigger]")).ToBeEnabledAsync();
        var image = page.Locator("[data-home-hero='photograph'] img");
        if (await image.CountAsync() > 0) await image.EvaluateAsync("el => el.dispatchEvent(new Event('error'))");
        await Expect(page.Locator("[data-home-hero='signature']")).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveCountAsync(1);
        await Expect(page.GetByText("当前摄影不可用，暂以极简签名展示。")).ToBeVisibleAsync();
    }
}
