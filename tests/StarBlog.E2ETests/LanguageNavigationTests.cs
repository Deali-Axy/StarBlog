using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace StarBlog.E2ETests;

public sealed class LanguageNavigationTests {
    [Theory]
    [InlineData(1280)]
    [InlineData(375)]
    public async Task Language_switch_persists_across_navigation_and_reload(int width) {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() {
            Headless = true, ExecutablePath = Environment.GetEnvironmentVariable("E2E_CHROMIUM_EXECUTABLE_PATH")
        });
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = width, Height = 900 } });
        await page.GotoAsync($"{baseUrl}/about?section=skills&lang=original#skills");
        await Expect(page.Locator("html")).ToHaveAttributeAsync("lang", "zh-CN");

        var switchButton = page.GetByRole(AriaRole.Button, new() { Name = "Switch to English", Exact = true });
        await Expect(switchButton).ToBeInViewportAsync();
        await switchButton.ClickAsync();
        await Expect(page.Locator("html")).ToHaveAttributeAsync("lang", "en");
        Assert.EndsWith("/about?section=skills#skills", page.Url);
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Skills", Exact = true })).ToBeVisibleAsync();

        await page.ReloadAsync();
        await Expect(page.Locator("html")).ToHaveAttributeAsync("lang", "en");
        await page.Locator("footer").GetByRole(AriaRole.Link, new() { Name = "RSS", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "Open Atom feed", Exact = true })).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "切换为中文", Exact = true }).ClickAsync();
        await Expect(page.Locator("html")).ToHaveAttributeAsync("lang", "zh-CN");
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "打开 Atom 源", Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Invalid_language_cookie_falls_back_to_Chinese() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() {
            Headless = true, ExecutablePath = Environment.GetEnvironmentVariable("E2E_CHROMIUM_EXECUTABLE_PATH")
        });
        var page = await browser.NewPageAsync();
        await page.Context.AddCookiesAsync([new Cookie { Name = "starblog.locale", Value = "invalid", Url = baseUrl }]);
        await page.GotoAsync($"{baseUrl}/about");
        await Expect(page.Locator("html")).ToHaveAttributeAsync("lang", "zh-CN");
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "技能", Exact = true })).ToBeVisibleAsync();
    }
}
