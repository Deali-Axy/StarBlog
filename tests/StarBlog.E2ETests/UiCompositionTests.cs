using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace StarBlog.E2ETests;

public sealed class UiCompositionTests {
    private static async Task<IBrowser> LaunchAsync(IPlaywright playwright) => await playwright.Chromium.LaunchAsync(new() {
        Headless = true, ExecutablePath = Environment.GetEnvironmentVariable("E2E_CHROMIUM_EXECUTABLE_PATH")
    });
    private static async Task<IPage> PageAsync(IBrowser browser, int width = 1440) {
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = width, Height = 1000 } });
        await E2EBaseUrl.UseApiOverrideAsync(page);
        return page;
    }
    [Fact]
    public async Task Article_drafts_and_encoded_heading_targets_survive_replacing_all_three_layouts() {
        if (!E2EBaseUrl.TryGet(out var url)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await PageAsync(browser);
        await page.GotoAsync($"{url}/blog/article-reading-fixture?bv=library");
        var content = page.Locator("textarea[maxlength='300']");
        await content.FillAsync("A draft that must survive changing the article tree.");
        await page.GetByPlaceholder("邮箱地址", new() { Exact = true }).FillAsync("draft@example.com");
        await page.GetByPlaceholder("昵称", new() { Exact = true }).FillAsync("Draft reader");
        await page.GetByPlaceholder("验证码", new() { Exact = true }).FillAsync("1234");
        foreach (var (id, label) in new[] { ("editorial", "折页手记"), ("studio", "档案工作台"), ("library", "活字索引") }) {
            await BlogPreview.SelectAsync(page, label);
            await Expect(page.Locator("[data-article-layout]")).ToHaveAttributeAsync("data-article-layout", id);
            await Expect(content).ToHaveValueAsync("A draft that must survive changing the article tree.");
            await Expect(page.GetByPlaceholder("邮箱地址", new() { Exact = true })).ToHaveValueAsync("draft@example.com");
            await Expect(page.GetByPlaceholder("昵称", new() { Exact = true })).ToHaveValueAsync("Draft reader");
            await Expect(page.GetByPlaceholder("验证码", new() { Exact = true })).ToHaveValueAsync("1234");
            await Expect(page.Locator("[data-article-body]")).ToHaveCountAsync(1);
            await Expect(page.Locator("h1").First).ToBeVisibleAsync();
            Assert.True(await page.EvaluateAsync<bool>("Array.from(document.querySelectorAll('[data-article-toc] a[href]')).every(a => document.getElementById(decodeURIComponent(a.hash.slice(1))))"));
        }
    }
    [Fact]
    public async Task Search_draft_and_archive_selection_survive_styles_and_collection_modes() {
        if (!E2EBaseUrl.TryGet(out var url)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await PageAsync(browser);
        await page.GotoAsync($"{url}/blog?bv=studio&pageSize=6");
        var second = page.Locator("[data-blog-article]").Nth(1);
        var selectedId = await second.GetAttributeAsync("data-blog-article");
        await second.GetByRole(AriaRole.Button).ClickAsync();
        await page.GetByRole(AriaRole.Searchbox, new() { Name = "搜索文章", Exact = true }).FillAsync("Unsubmitted C# draft");
        await page.GetByRole(AriaRole.Link, new() { Name = "紧凑索引", Exact = true }).ClickAsync();
        await Expect(second).ToHaveAttributeAsync("data-selected", "true");
        foreach (var label in new[] { "活字索引", "折页手记", "档案工作台" }) {
            await BlogPreview.SelectAsync(page, label);
            await Expect(page.GetByRole(AriaRole.Searchbox, new() { Name = "搜索文章", Exact = true })).ToHaveValueAsync("Unsubmitted C# draft");
        }
        await Expect(page.Locator($"[data-blog-article='{selectedId}']")).ToHaveAttributeAsync("data-selected", "true");
        await page.GetByRole(AriaRole.Link, new() { Name = "展开阅读", Exact = true }).ClickAsync();
        await Expect(page.Locator($"[data-blog-article='{selectedId}']")).ToHaveAttributeAsync("data-selected", "true");
    }
    [Fact]
    public async Task Every_home_slot_can_be_replaced_without_losing_the_editor_or_expanded_summary() {
        if (!E2EBaseUrl.TryGet(out var url)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await PageAsync(browser);
        await page.GotoAsync(url);
        var summary = page.Locator("[data-home-section='latestPosts'] button[aria-expanded]").First;
        await summary.ClickAsync();
        await page.Locator("[data-home-design-trigger]").ClickAsync();
        var panel = page.Locator("[data-home-design-menu]");
        await panel.GetByRole(AriaRole.Tab, new() { Name = "内容模块", Exact = true }).ClickAsync();
        var choices = new Dictionary<string, string[]> {
            ["featuredPosts"] = ["notes", "editorial"], ["latestPosts"] = ["ledger", "timeline"],
            ["interests"] = ["tags", "cloud"], ["categories"] = ["catalog", "accordion", "rail", "tilt", "deck"],
            ["photos"] = ["prints", "contactSheet"], ["links"] = ["cards", "directory"]
        };
        foreach (var (slot, values) in choices) foreach (var value in values) {
            await panel.Locator($"[data-home-setting='sections.{slot}'] input[value='{value}']").CheckAsync();
            await Expect(panel.GetByRole(AriaRole.Tab, new() { Name = "内容模块", Exact = true })).ToHaveAttributeAsync("data-state", "active");
            await Expect(page.Locator("[data-home-section='latestPosts'] [data-expanded='true']")).ToHaveCountAsync(1);
            await Expect(page.Locator("main h1")).ToHaveCountAsync(1);
        }
        await panel.GetByRole(AriaRole.Tab, new() { Name = "页面布局", Exact = true }).ClickAsync();
        foreach (var frame in new[] { "topbar", "sidebar" }) {
            await panel.Locator($"[data-home-setting='frame'] input[value='{frame}']").CheckAsync();
            await Expect(panel).ToBeVisibleAsync();
            await Expect(page.Locator("[data-home-section='latestPosts'] [data-expanded='true']")).ToHaveCountAsync(1);
        }
        await page.Keyboard.PressAsync("Escape");
        await Expect(page.Locator("[data-home-design-trigger]")).ToBeFocusedAsync();
    }
    [Theory]
    [InlineData(375)] [InlineData(768)] [InlineData(1440)]
    public async Task Both_photo_layouts_keep_the_same_photos_at_every_density(int width) {
        if (!E2EBaseUrl.TryGet(out var url)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await PageAsync(browser, width);
        await page.GotoAsync($"{url}/photography");
        var wall = page.Locator("[data-photo-wall]");
        await Expect(wall.Locator("[data-photo-id]").First).ToBeVisibleAsync();
        var ids = await wall.Locator("[data-photo-id]").EvaluateAllAsync<string[]>("els => els.map(el => el.dataset.photoId)");
        foreach (var (view, label) in new[] { ("justified", "齐行"), ("masonry", "瀑布流") }) {
            await page.GetByRole(AriaRole.Button, new() { Name = label, Exact = true }).ClickAsync();
            await Expect(wall).ToHaveAttributeAsync("data-view", view);
            foreach (var (density, name) in new[] { ("comfortable", "宽松"), ("compact", "紧凑"), ("standard", "标准") }) {
                await page.GetByRole(AriaRole.Combobox, new() { Name = "照片密度", Exact = true }).ClickAsync();
                await page.GetByRole(AriaRole.Option, new() { Name = name, Exact = true }).ClickAsync();
                await Expect(wall).ToHaveAttributeAsync("data-density", density);
                Assert.Equal(ids, await wall.Locator("[data-photo-id]").EvaluateAllAsync<string[]>("els => els.map(el => el.dataset.photoId)"));
                Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
            }
        }
    }
}
