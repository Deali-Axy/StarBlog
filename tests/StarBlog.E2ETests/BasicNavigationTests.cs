using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace StarBlog.E2ETests;

public sealed class BasicNavigationTests {
    [Fact]
    public async Task HomePage_HasTitleAndSiteBrand() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions {
            Headless = true, ExecutablePath = Environment.GetEnvironmentVariable("E2E_CHROMIUM_EXECUTABLE_PATH")
        });
        var page = await browser.NewPageAsync();
        await E2EBaseUrl.UseApiOverrideAsync(page);

        await page.GotoAsync($"{baseUrl}/", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        var title = await page.TitleAsync();

        Assert.False(string.IsNullOrWhiteSpace(title));
        await Expect(page.Locator("header").GetByRole(AriaRole.Link, new() { Name = "StarBlog", Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task RobotsTxt_ContainsSitemap() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions {
            Headless = true, ExecutablePath = Environment.GetEnvironmentVariable("E2E_CHROMIUM_EXECUTABLE_PATH")
        });
        var page = await browser.NewPageAsync();
        await E2EBaseUrl.UseApiOverrideAsync(page);

        await page.GotoAsync($"{baseUrl}/robots.txt", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        var text = await page.EvaluateAsync<string>("() => document.body ? document.body.innerText : ''");

        Assert.Contains("Sitemap:", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/sitemap.xml", text, StringComparison.OrdinalIgnoreCase);
    }
}
