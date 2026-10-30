using System.Text.Json;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace StarBlog.E2ETests;

public sealed class BlogNavigationTests {
    private sealed record CategoryNode(int Id, string Text, CategoryNode[]? Nodes);

    private static Task<IBrowser> LaunchAsync(IPlaywright playwright) => playwright.Chromium.LaunchAsync(new() {
        Headless = true,
        ExecutablePath = Environment.GetEnvironmentVariable("E2E_CHROMIUM_EXECUTABLE_PATH")
    });

    private static async Task<IPage> NewPageAsync(IBrowser browser, int width = 1440) {
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = width, Height = 1000 } });
        await E2EBaseUrl.UseApiOverrideAsync(page);
        return page;
    }

    private static async Task<CategoryNode[]> CategoriesAsync(IPage page, string baseUrl) {
        var apiUrl = Environment.GetEnvironmentVariable("E2E_API_BASE_URL") ?? baseUrl;
        var response = await page.APIRequest.GetAsync($"{apiUrl}/api/v1/categories/tree");
        Assert.True(response.Ok);
        return JsonSerializer.Deserialize<CategoryNode[]>(await response.TextAsync(), JsonSerializerOptions.Web)!;
    }

    public static IEnumerable<object[]> DesignCases() {
        foreach (var variant in new[] { "library", "editorial", "studio" })
        foreach (var width in new[] { 375, 768, 1440 })
        foreach (var theme in new[] { "light", "dark" })
            yield return [variant, width, theme];
    }

    [Theory]
    [MemberData(nameof(DesignCases))]
    public async Task Designs_are_responsive_and_portals_inherit_their_theme(string variant, int width, string theme) {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser, width);
        await page.AddInitScriptAsync($"localStorage.setItem('theme', '{theme}')");
        await page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);
        await page.GotoAsync($"{baseUrl}/blog?bv={variant}");
        var shell = page.Locator("[data-blog-variant]");
        await Expect(shell).ToHaveAttributeAsync("data-blog-variant", variant);
        var title = variant == "library" ? "文章索引" : variant == "editorial" ? "折页手记" : "档案工作台";
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = title, Exact = true })).ToBeVisibleAsync();
        await Expect(page.Locator("[data-blog-layout]")).ToHaveAttributeAsync("data-blog-layout", variant == "library" ? "index" : variant == "editorial" ? "folio" : "archive");
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
        var cardBackground = await shell.EvaluateAsync<string>("el => getComputedStyle(el).getPropertyValue('--card').trim()");
        await page.GetByRole(AriaRole.Combobox, new() { Name = "排序", Exact = true }).ClickAsync();
        var listbox = page.GetByRole(AriaRole.Listbox);
        var hex = cardBackground.TrimStart('#');
        if (hex.Length == 3) hex = string.Concat(hex.Select(character => new string(character, 2)));
        var color = Convert.FromHexString(hex);
        var expectedBackground = $"rgb({color[0]}, {color[1]}, {color[2]})";
        await Expect(listbox).ToHaveCSSAsync("background-color", expectedBackground);
        await Expect(page.GetByRole(AriaRole.Option, new() { Name = "更新时间", Exact = true })).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Escape");
        await Expect(page.GetByRole(AriaRole.Combobox, new() { Name = "排序", Exact = true })).ToBeFocusedAsync();
        var open = page.GetByRole(AriaRole.Button, new() { Name = "打开分类目录", Exact = true });
        if (width < 1024 || variant != "studio") {
            await open.ClickAsync();
            var dialog = page.GetByRole(AriaRole.Dialog);
            await Expect(dialog).ToBeVisibleAsync();
            await Expect(dialog).ToHaveCSSAsync("background-color", expectedBackground);
            Assert.True(await dialog.EvaluateAsync<bool>("el => el.contains(document.activeElement)"));
            await page.Keyboard.PressAsync("Shift+Tab");
            Assert.True(await dialog.EvaluateAsync<bool>("el => el.contains(document.activeElement)"));
            await page.Keyboard.PressAsync("Escape");
            await Expect(dialog).ToBeHiddenAsync();
            await Expect(open).ToBeFocusedAsync();
        } else {
            await Expect(open).ToBeHiddenAsync();
            var tree = page.GetByRole(AriaRole.Navigation, new() { Name = "分类目录", Exact = true });
            Assert.True(await tree.EvaluateAsync<bool>("el => el.getBoundingClientRect().bottom <= innerHeight"));
        }
        await page.Locator("main h1").ClickAsync();
        var screenshotDirectory = Environment.GetEnvironmentVariable("E2E_SCREENSHOT_DIR");
        if (!string.IsNullOrWhiteSpace(screenshotDirectory)) {
            Directory.CreateDirectory(screenshotDirectory);
            await page.ScreenshotAsync(new() { Path = Path.Combine(screenshotDirectory, $"blog-{variant}-{width}-{theme}.png") });
        }
        await page.GetByRole(AriaRole.Link, new() { Name = "紧凑索引", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "紧凑索引", Exact = true })).ToHaveAttributeAsync("aria-current", "true");
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
        Assert.Empty(errors);
    }

    [Fact]
    public async Task Categories_start_collapsed_search_restores_state_and_deep_links_expand_ancestors() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser);
        var categories = await CategoriesAsync(page, baseUrl);
        var parent = categories.First(node => node.Nodes?.Length > 0);
        var child = parent.Nodes![0];
        var other = categories.First(node => node.Id != parent.Id && node.Nodes?.Length > 0);
        await page.GotoAsync($"{baseUrl}/blog");
        await page.GetByRole(AriaRole.Button, new() { Name = "打开分类目录", Exact = true }).ClickAsync();
        var tree = page.GetByRole(AriaRole.Navigation, new() { Name = "分类目录", Exact = true });
        var parentNode = tree.Locator($"[data-category-id='{parent.Id}']");
        await Expect(parentNode).ToHaveAttributeAsync("data-state", "closed");
        await Expect(tree.Locator($"[data-category-id='{child.Id}']")).ToBeHiddenAsync();
        await tree.GetByRole(AriaRole.Button, new() { Name = $"展开分类: {other.Text}", Exact = true }).ClickAsync();
        await tree.GetByRole(AriaRole.Searchbox).FillAsync(child.Text);
        await Expect(tree.Locator($"[data-category-id='{child.Id}']")).ToBeVisibleAsync();
        await tree.GetByRole(AriaRole.Button, new() { Name = "清除分类搜索", Exact = true }).ClickAsync();
        await Expect(parentNode).ToHaveAttributeAsync("data-state", "closed");
        await Expect(tree.Locator($"[data-category-id='{other.Id}']")).ToHaveAttributeAsync("data-state", "open");
        await page.Keyboard.PressAsync("Escape");
        await BlogPreview.SelectAsync(page, "档案工作台");
        await Expect(tree.Locator($"[data-category-id='{other.Id}']")).ToHaveAttributeAsync("data-state", "open");
        await page.GotoAsync($"{baseUrl}/blog?categoryId={child.Id}&bv=editorial");
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = child.Text, Exact = true })).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "打开分类目录", Exact = true }).ClickAsync();
        await Expect(parentNode).ToHaveAttributeAsync("data-state", "open");
        await Expect(tree.Locator($"[data-category-id='{child.Id}'] a")).ToHaveAttributeAsync("aria-current", "page");
        // Selecting another branch supplements expansion without discarding the old branch.
        await tree.GetByRole(AriaRole.Button, new() { Name = $"展开分类: {other.Text}", Exact = true }).ClickAsync();
        await tree.Locator($"[data-category-id='{other.Nodes![0].Id}'] a").ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = other.Nodes[0].Text, Exact = true })).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "打开分类目录", Exact = true }).ClickAsync();
        await Expect(parentNode).ToHaveAttributeAsync("data-state", "open");
        await Expect(tree.Locator($"[data-category-id='{other.Id}']")).ToHaveAttributeAsync("data-state", "open");
    }

    [Fact]
    public async Task Preview_search_sort_paging_and_browser_history_keep_query_settings() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser);
        await page.GotoAsync($"{baseUrl}/blog?page=2&pageSize=6&view=list&sortBy=title&sortType=asc&bv=library");
        var articles = await page.Locator("[data-blog-article]").EvaluateAllAsync<string[]>("els => els.map(el => el.dataset.blogArticle)");
        await BlogPreview.SelectAsync(page, "折页手记");
        await Expect(page.Locator("[data-blog-variant]")).ToHaveAttributeAsync("data-blog-variant", "editorial");
        Assert.Contains("page=2", page.Url);
        Assert.Equal(articles, await page.Locator("[data-blog-article]").EvaluateAllAsync<string[]>("els => els.map(el => el.dataset.blogArticle)"));
        await page.GetByRole(AriaRole.Link, new() { Name = "下一页", Exact = true }).ClickAsync();
        await page.WaitForURLAsync("**/blog?*page=3*");
        Assert.Contains("bv=editorial", page.Url);
        Assert.Contains("view=list", page.Url);
        await page.GetByRole(AriaRole.Link, new() { Name = "展开阅读", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "展开阅读", Exact = true })).ToHaveAttributeAsync("aria-current", "true");
        Assert.Contains("page=3", page.Url);
        await page.GoBackAsync();
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "紧凑索引", Exact = true })).ToHaveAttributeAsync("aria-current", "true");
        await page.GoBackAsync();
        await page.WaitForURLAsync("**/blog?*page=2*");
        await page.GoBackAsync();
        await Expect(page.Locator("[data-blog-variant]")).ToHaveAttributeAsync("data-blog-variant", "library");
        await page.GoForwardAsync();
        await Expect(page.Locator("[data-blog-variant]")).ToHaveAttributeAsync("data-blog-variant", "editorial");
        await page.ReloadAsync();
        await Expect(page.Locator("[data-blog-variant]")).ToHaveAttributeAsync("data-blog-variant", "editorial");
        await page.GetByRole(AriaRole.Combobox, new() { Name = "排序", Exact = true }).ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = "创建时间", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Combobox, new() { Name = "排序", Exact = true })).ToHaveTextAsync("创建时间");
        Assert.DoesNotContain("page=", page.Url);
        Assert.Contains("pageSize=6", page.Url);
        await page.GetByRole(AriaRole.Link, new() { Name = "下一页", Exact = true }).ClickAsync();
        await page.WaitForURLAsync("**/blog?*page=2*");
        await page.GetByRole(AriaRole.Searchbox, new() { Name = "搜索文章", Exact = true }).FillAsync("blog_no_match_472af617");
        await page.GetByRole(AriaRole.Button, new() { Name = "搜索", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "这片星空里暂时没有匹配的文章。", Exact = true })).ToBeVisibleAsync();
        Assert.DoesNotContain("page=", page.Url);
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "移除搜索条件: blog_no_match_472af617", Exact = true })).ToHaveAttributeAsync("href", new System.Text.RegularExpressions.Regex("bv=editorial"));
        await page.GetByRole(AriaRole.Link, new() { Name = "清除筛选", Exact = true }).First.ClickAsync();
        await Expect(page.Locator("[data-blog-article]").First).ToBeVisibleAsync();
        Assert.Contains("view=list", page.Url);
        Assert.Contains("sortBy=creationTime", page.Url);
        Assert.Contains("sortType=asc", page.Url);
        Assert.Contains("bv=editorial", page.Url);
        await page.GetByRole(AriaRole.Link, new() { Name = "下一页", Exact = true }).ClickAsync();
        await page.WaitForURLAsync("**/blog?*page=2*");
        await page.GetByRole(AriaRole.Link, new() { Name = "每页 24", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "每页 24", Exact = true })).ToHaveAttributeAsync("aria-current", "true");
        Assert.DoesNotContain("page=", page.Url);
        await page.GetByRole(AriaRole.Button, new() { Name = "打开分类目录", Exact = true }).ClickAsync();
        var tree = page.GetByRole(AriaRole.Navigation, new() { Name = "分类目录", Exact = true });
        await tree.GetByRole(AriaRole.Link, new() { Name = "全部文章", Exact = true }).ClickAsync();
        Assert.Contains("pageSize=24", page.Url);
        await page.GetByRole(AriaRole.Button, new() { Name = "Switch to English", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Design preview", Exact = true })).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Field Notes", Exact = true })).ToBeVisibleAsync();
        Assert.Contains("bv=editorial", page.Url);
    }

    [Fact]
    public async Task Pending_navigation_reports_loading_until_results_arrive() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser);
        await page.GotoAsync($"{baseUrl}/blog?bv=studio");
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await page.RouteAsync("**/blog?*", async route => {
            if (route.Request.Url.Contains("view=list") && route.Request.Headers.ContainsKey("rsc")) await release.Task;
            await route.ContinueAsync();
        });
        var click = page.GetByRole(AriaRole.Link, new() { Name = "紧凑索引", Exact = true }).ClickAsync();
        try {
            await Expect(page.GetByRole(AriaRole.Status)).ToHaveTextAsync("正在加载文章…");
            await Expect(page.GetByRole(AriaRole.Region, new() { Name = "文章", Exact = true })).ToHaveAttributeAsync("aria-busy", "true");
        } finally {
            release.TrySetResult();
            await click;
        }
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "紧凑索引", Exact = true })).ToHaveAttributeAsync("aria-current", "true");
        await Expect(page.GetByRole(AriaRole.Region, new() { Name = "文章", Exact = true })).ToHaveAttributeAsync("aria-busy", "false");
    }

    [Fact]
    public async Task Fixture_covers_long_titles_missing_summaries_deep_paths_empty_categories_and_last_page() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser, 375);
        var root = (await CategoriesAsync(page, baseUrl)).FirstOrDefault(node => node.Text == "Blog E2E root");
        if (root is null) return; // Optional isolated fixture; see the browser-test README.
        var branch = root.Nodes!.Single(node => node.Text == "Blog E2E branch");
        var leaf = branch.Nodes![0];
        var empty = root.Nodes!.Single(node => node.Text == "Blog E2E empty");
        foreach (var variant in new[] { "library", "editorial", "studio" }) {
            await page.GotoAsync($"{baseUrl}/blog?categoryId={leaf.Id}&bv={variant}&pageSize=6");
            await Expect(page.Locator("[data-blog-article]")).ToHaveCountAsync(6);
            var first = page.Locator("[data-blog-article]").First;
            Assert.True((await first.GetByRole(AriaRole.Heading).InnerTextAsync()).Length > 120);
            await Expect(first.Locator("p")).ToHaveCountAsync(0);
            Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
            await page.GetByRole(AriaRole.Button, new() { Name = "打开分类目录", Exact = true }).ClickAsync();
            var dialog = page.GetByRole(AriaRole.Dialog);
            await Expect(dialog.Locator($"[data-category-id='{root.Id}']")).ToHaveAttributeAsync("data-state", "open");
            await Expect(dialog.Locator($"[data-category-id='{branch.Id}']")).ToHaveAttributeAsync("data-state", "open");
            await Expect(dialog.Locator($"[data-category-id='{leaf.Id}'] a")).ToHaveAttributeAsync("aria-current", "page");
            await page.Keyboard.PressAsync("Escape");
            await page.GetByRole(AriaRole.Link, new() { Name = "第 5 / 5 页", Exact = true }).ClickAsync();
            await Expect(page.Locator("[data-blog-article]")).ToHaveCountAsync(1);
            await Expect(page.GetByRole(AriaRole.Button, new() { Name = "下一页", Exact = true })).ToBeDisabledAsync();
            await page.GotoAsync($"{baseUrl}/blog?categoryId={leaf.Id}&bv={variant}&pageSize=6&page=6");
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "这一页还没有文章", Exact = true })).ToBeVisibleAsync();
            await page.GetByRole(AriaRole.Link, new() { Name = "返回第一页", Exact = true }).ClickAsync();
            await Expect(page.Locator("[data-blog-article]")).ToHaveCountAsync(6);
            await page.GotoAsync($"{baseUrl}/blog?categoryId={empty.Id}&bv={variant}");
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "这片星空里暂时没有匹配的文章。", Exact = true })).ToBeVisibleAsync();
            await Expect(page.Locator("[data-blog-article]")).ToHaveCountAsync(0);
        }
    }

    [Fact]
    public async Task Blog_layout_follows_articles_but_does_not_leak_into_home_or_photography() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser);
        await page.AddInitScriptAsync("localStorage.setItem('theme', 'light')");
        await page.GotoAsync($"{baseUrl}/blog?bv=studio");
        await page.Locator("[data-blog-article] a").First.ClickAsync();
        await Expect(page.Locator("main h1").First).ToBeVisibleAsync();
        await Expect(page.Locator("[data-blog-chrome]")).ToHaveAttributeAsync("data-blog-chrome", "studio");
        await Expect(page.Locator("[data-blog-variant]")).ToHaveCountAsync(0);
        await page.GetByRole(AriaRole.Banner).GetByRole(AriaRole.Link, new() { Name = "首页", Exact = true }).ClickAsync();
        await page.WaitForURLAsync($"{baseUrl}/");
        await Expect(page.Locator("[data-blog-chrome]")).ToHaveCountAsync(0);
        await Expect(page.Locator("main h1")).ToBeVisibleAsync();
        await Expect(page.Locator("[data-blog-variant]")).ToHaveCountAsync(0);
        await page.GetByRole(AriaRole.Banner).GetByRole(AriaRole.Link, new() { Name = "摄影", Exact = true }).ClickAsync();
        await Expect(page.Locator("[data-photography-theme]").First).ToHaveCSSAsync("background-color", "rgb(255, 255, 255)");
        await Expect(page.Locator("[data-blog-chrome]")).ToHaveCountAsync(0);
        await Expect(page.Locator("[data-blog-variant]")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Mobile_selection_closes_drawer_and_invalid_design_falls_back() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser, 375);
        await page.GotoAsync($"{baseUrl}/blog?bv=invalid&view=list&pageSize=6");
        await Expect(page.Locator("[data-blog-variant]")).ToHaveAttributeAsync("data-blog-variant", "library");
        await BlogPreview.SelectAsync(page, "档案工作台");
        var open = page.GetByRole(AriaRole.Button, new() { Name = "打开分类目录", Exact = true });
        await open.ClickAsync();
        var dialog = page.GetByRole(AriaRole.Dialog);
        var categories = await CategoriesAsync(page, baseUrl);
        var selected = categories[0];
        await dialog.GetByRole(AriaRole.Link, new() { Name = selected.Text, Exact = true }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = selected.Text, Exact = true })).ToBeVisibleAsync();
        await Expect(open).ToContainTextAsync(selected.Text);
        await Expect(open).ToBeFocusedAsync();
        Assert.Contains("bv=studio", page.Url);
        Assert.Contains("view=list", page.Url);
        Assert.Contains("pageSize=6", page.Url);
    }

    [Fact]
    public async Task Layouts_have_distinct_compositions_and_archive_keyboard_selection_updates_preview() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser);
        await page.GotoAsync($"{baseUrl}/blog?bv=library");
        var articles = page.Locator("[data-blog-article]");
        var first = (await articles.Nth(0).BoundingBoxAsync())!;
        var second = (await articles.Nth(1).BoundingBoxAsync())!;
        Assert.True(first.Width > 1100 && second.Y >= first.Y + first.Height - 1);
        await BlogPreview.SelectAsync(page, "折页手记");
        first = (await articles.Nth(0).BoundingBoxAsync())!;
        second = (await articles.Nth(1).BoundingBoxAsync())!;
        Assert.True(Math.Abs(first.Y - second.Y) < 2 && first.Width > second.Width * 1.2);
        await BlogPreview.SelectAsync(page, "档案工作台");
        var preview = page.GetByRole(AriaRole.Complementary, new() { Name = "文章预览", Exact = true });
        await Expect(preview).ToBeVisibleAsync();
        var title = await articles.Nth(1).GetByRole(AriaRole.Heading).InnerTextAsync();
        var selection = articles.Nth(0).GetByRole(AriaRole.Button);
        await selection.FocusAsync();
        await page.Keyboard.PressAsync("ArrowDown");
        await Expect(articles.Nth(1).GetByRole(AriaRole.Button)).ToBeFocusedAsync();
        await Expect(articles.Nth(1)).ToHaveAttributeAsync("data-selected", "true");
        await Expect(preview.GetByRole(AriaRole.Heading)).ToHaveTextAsync(title);
        await page.Keyboard.PressAsync("End");
        await Expect(articles.Last.GetByRole(AriaRole.Button)).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Home");
        await Expect(selection).ToBeFocusedAsync();
        await page.GetByRole(AriaRole.Link, new() { Name = "下一页", Exact = true }).ClickAsync();
        await page.WaitForURLAsync("**/blog?*page=2*");
        await Expect(articles.First).ToHaveAttributeAsync("data-selected", "true");
        await page.SetViewportSizeAsync(1440, 768);
        Assert.True(await preview.EvaluateAsync<bool>("el => el.getBoundingClientRect().bottom <= innerHeight"));
        Assert.True(await page.GetByRole(AriaRole.Navigation, new() { Name = "分类目录", Exact = true }).EvaluateAsync<bool>("el => el.getBoundingClientRect().bottom <= innerHeight"));
        await Expect(preview.GetByRole(AriaRole.Heading)).ToHaveTextAsync(await articles.First.GetByRole(AriaRole.Heading).InnerTextAsync());
    }

    [Theory]
    [InlineData(375)]
    [InlineData(768)]
    public async Task Archive_mobile_preview_traps_focus_closes_with_escape_and_returns_focus(int width) {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser, width);
        await page.GotoAsync($"{baseUrl}/blog?bv=studio");
        var article = page.Locator("[data-blog-article]").Nth(1);
        var selection = article.GetByRole(AriaRole.Button);
        var title = await article.GetByRole(AriaRole.Heading).InnerTextAsync();
        var url = page.Url;
        var articleHref = await article.GetByRole(AriaRole.Link).GetAttributeAsync("href");
        await selection.ClickAsync();
        var dialog = page.GetByRole(AriaRole.Dialog, new() { Name = title, Exact = true });
        await Expect(dialog).ToBeVisibleAsync();
        await Expect(dialog.GetByRole(AriaRole.Heading)).ToHaveTextAsync(title);
        await Expect(dialog.GetByRole(AriaRole.Link, new() { Name = "阅读全文", Exact = true })).ToHaveAttributeAsync("href", articleHref!);
        await page.Keyboard.PressAsync("Shift+Tab");
        Assert.True(await dialog.EvaluateAsync<bool>("el => el.contains(document.activeElement)"));
        await page.Keyboard.PressAsync("Escape");
        await Expect(dialog).ToBeHiddenAsync();
        await Expect(selection).ToBeFocusedAsync();
        Assert.Equal(url, page.Url);
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
    }

    [Theory]
    [InlineData("library", "The Index")]
    [InlineData("editorial", "Field Notes")]
    [InlineData("studio", "The Archive")]
    public async Task English_designs_and_category_dialogs_keep_their_navigation(string variant, string title) {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser, 375);
        await page.GotoAsync($"{baseUrl}/blog?bv={variant}&pageSize=6");
        await page.GetByRole(AriaRole.Button, new() { Name = "Switch to English", Exact = true }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = title, Exact = true })).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Open article categories", Exact = true }).ClickAsync();
        var dialog = page.GetByRole(AriaRole.Dialog);
        await Expect(dialog.GetByRole(AriaRole.Heading, new() { Name = "Topics", Exact = true })).ToBeVisibleAsync();
        await dialog.GetByRole(AriaRole.Link, new() { Name = "All articles", Exact = true }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();
        Assert.Contains($"bv={variant}", page.Url);
        Assert.Contains("pageSize=6", page.Url);
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > innerWidth"));
    }
}
