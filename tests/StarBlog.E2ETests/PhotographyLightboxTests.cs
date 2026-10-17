using System.Collections.Concurrent;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace StarBlog.E2ETests;

public sealed partial class PhotographyNavigationTests {
    private static TaskCompletionSource NewGate() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static async Task OpenLightboxAsync(IPage page, string baseUrl, int index = 0) {
        await page.GotoAsync($"{baseUrl}/photography");
        await page.Locator("[data-photo-id] a").Nth(index).ClickAsync();
        await page.WaitForFunctionAsync("() => { const img = document.querySelector('[role=dialog] img'); return img?.complete && img.naturalWidth > 0 }");
        await page.GetByRole(AriaRole.Button, new() { Name = "查看大图", Exact = true }).ClickAsync();
        await Expect(page.Locator("[data-lightbox-stage]")).ToBeVisibleAsync();
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
    public async Task Slow_originals_keep_a_black_stable_stage_and_visible_preview(int width, string theme) {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser, width);
        await page.AddInitScriptAsync($"localStorage.setItem('theme', '{theme}')");
        await page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.NoPreference });
        var first = NewGate();
        var following = NewGate();
        string? firstPath = null;
        await page.RouteAsync("**/media/photography/**", async route => {
            var path = new Uri(route.Request.Url).AbsolutePath;
            firstPath ??= path;
            await (path == firstPath ? first.Task : following.Task);
            if (!page.IsClosed) await route.FallbackAsync();
        });
        try {
            await OpenLightboxAsync(page, baseUrl);
            var stage = page.Locator("[data-lightbox-stage]");
            await Expect(stage).ToHaveAttributeAsync("data-state", "loading");
            await Expect(stage).ToHaveCSSAsync("background-color", "rgb(0, 0, 0)");
            var before = await stage.BoundingBoxAsync() ?? throw new InvalidOperationException("Missing image stage");
            Assert.True(before.Width > 160);
            Assert.Equal(804, before.Height);
            await Expect(stage.Locator("[data-lightbox-preview]")).ToHaveCSSAsync("opacity", "1");
            await page.WaitForTimeoutAsync(180);
            await Expect(stage.Locator("[data-lightbox-loading]")).ToHaveCountAsync(0);
            first.TrySetResult();
            await Expect(stage).ToHaveAttributeAsync("data-state", "ready");
            await Expect(stage.Locator("[data-lightbox-original]")).ToBeVisibleAsync();
            var after = await stage.BoundingBoxAsync() ?? throw new InvalidOperationException("Missing image stage");
            Assert.Equal(before.Width, after.Width);
            Assert.Equal(before.Height, after.Height);
            var oldUrl = page.Url;
            await page.Keyboard.PressAsync("ArrowRight");
            await page.WaitForURLAsync(url => url != oldUrl);
            await Expect(stage).ToHaveAttributeAsync("data-state", "loading");
            await Expect(stage).ToHaveCSSAsync("background-color", "rgb(0, 0, 0)");
            await Expect(stage.Locator("[data-lightbox-preview]")).ToHaveCSSAsync("opacity", "1");
            var next = await stage.BoundingBoxAsync() ?? throw new InvalidOperationException("Missing image stage");
            Assert.Equal(before.Width, next.Width);
            Assert.Equal(before.Height, next.Height);
            following.TrySetResult();
            await Expect(stage).ToHaveAttributeAsync("data-state", "ready");
            Assert.EndsWith(await stage.GetAttributeAsync("data-lightbox-photo") ?? "missing", page.Url);
        } finally {
            first.TrySetResult();
            following.TrySetResult();
        }
    }

    [Fact]
    public async Task Failed_original_keeps_the_preview_and_retries_only_that_original() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser);
        var requests = new ConcurrentQueue<string>();
        string? firstPath = null;
        var fail = true;
        await page.RouteAsync("**/media/photography/**", async route => {
            var path = new Uri(route.Request.Url).AbsolutePath;
            requests.Enqueue(path);
            firstPath ??= path;
            if (fail && path == firstPath) await route.FulfillAsync(new() { Status = 503, Body = "Unavailable" });
            else await route.FallbackAsync();
        });
        await OpenLightboxAsync(page, baseUrl, 8);
        var stage = page.Locator("[data-lightbox-stage]");
        await Expect(stage).ToHaveAttributeAsync("data-state", "failed");
        var preview = stage.Locator("[data-lightbox-preview]");
        var previewSrc = await preview.GetAttributeAsync("src");
        await Expect(preview).ToHaveCSSAsync("opacity", "1");
        await Expect(stage.Locator("[data-lightbox-error]")).ToHaveCSSAsync("color", "rgb(255, 255, 255)");
        fail = false;
        await stage.GetByRole(AriaRole.Button, new() { NameRegex = new("^重试:") }).ClickAsync();
        await Expect(stage).ToHaveAttributeAsync("data-state", "ready");
        await Expect(stage.Locator("[data-lightbox-error]")).ToHaveCountAsync(0);
        Assert.Equal(previewSrc, await preview.GetAttributeAsync("src"));
        Assert.Equal(2, requests.Count(path => path == firstPath));
    }

    [Fact]
    public async Task Missing_preview_and_original_use_black_errors_and_leave_navigation_available() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser);
        string? firstPath = null;
        var failThumbnails = false;
        await page.RouteAsync("**/media/photography/**", async route => {
            var path = new Uri(route.Request.Url).AbsolutePath;
            firstPath ??= path;
            if (path != firstPath) await route.FulfillAsync(new() { Status = 503, Body = "Unavailable" });
            else await route.FallbackAsync();
        });
        await page.RouteAsync("**/api/v1/photos/*/thumbnail?*", async route => {
            if (failThumbnails) await route.AbortAsync();
            else await route.FallbackAsync();
        });
        await OpenLightboxAsync(page, baseUrl, 8);
        var stage = page.Locator("[data-lightbox-stage]");
        await Expect(stage).ToHaveAttributeAsync("data-state", "ready");
        failThumbnails = true;
        var url = page.Url;
        await page.Keyboard.PressAsync("ArrowRight");
        await page.WaitForURLAsync(current => current != url);
        await Expect(stage).ToHaveAttributeAsync("data-state", "failed");
        await Expect(stage.Locator("[data-lightbox-preview]")).ToHaveCSSAsync("opacity", "0");
        await Expect(stage.Locator("[data-lightbox-error]")).ToHaveCSSAsync("background-color", "rgb(0, 0, 0)");
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "上一张", Exact = true })).ToBeEnabledAsync();
        await page.Keyboard.PressAsync("Escape");
        await Expect(stage).ToHaveCountAsync(0);
        await Expect(page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Late_decodes_do_not_replace_new_photos_and_reopening_uses_the_decoded_cache() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser);
        var errors = new ConcurrentQueue<string>();
        page.PageError += (_, error) => errors.Enqueue(error);
        await page.AddInitScriptAsync("""
            window.lightboxDecodes = {};
            const decode = HTMLImageElement.prototype.decode;
            HTMLImageElement.prototype.decode = function () {
              return decode.call(this).then(() => {
                const path = new URL(this.src).pathname;
                if (path.startsWith('/media/photography/')) {
                  return new Promise(resolve => { window.lightboxDecodes[path] = resolve; });
                }
              });
            };
            """);
        var originals = new ConcurrentQueue<string>();
        page.Request += (_, request) => {
            var path = new Uri(request.Url).AbsolutePath;
            if (path.StartsWith("/media/photography/")) originals.Enqueue(path);
        };
        await page.GotoAsync($"{baseUrl}/photography");
        var ids = await page.Locator("[data-photo-id]").EvaluateAllAsync<string[]>("els => els.slice(6, 11).map(el => el.dataset.photoId)");
        var paths = await page.EvaluateAsync<string[]>("async ids => Promise.all(ids.map(async id => '/media/photography/' + (await (await fetch('/api/v1/photos/' + id)).json()).filePath))", ids);
        await page.Locator("[data-photo-id] a").Nth(7).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
        Assert.Empty(originals);
        await page.GetByRole(AriaRole.Button, new() { Name = "查看大图", Exact = true }).ClickAsync();
        var stage = page.Locator("[data-lightbox-stage]");
        await page.WaitForFunctionAsync("path => Boolean(window.lightboxDecodes[path])", paths[1]);
        await Expect(stage).ToHaveAttributeAsync("data-state", "loading");
        Assert.Single(originals);
        var oldUrl = page.Url;
        await page.Keyboard.PressAsync("ArrowRight");
        await page.WaitForURLAsync(url => url != oldUrl);
        await page.WaitForFunctionAsync("path => Boolean(window.lightboxDecodes[path])", paths[2]);
        await page.EvaluateAsync("path => window.lightboxDecodes[path]()", paths[2]);
        await Expect(stage).ToHaveAttributeAsync("data-state", "ready");
        await page.WaitForFunctionAsync("path => Boolean(window.lightboxDecodes[path])", paths[3]);
        Assert.Equal(3, originals.Count);
        await page.EvaluateAsync("path => window.lightboxDecodes[path]()", paths[1]);
        await Expect(stage).ToHaveAttributeAsync("data-lightbox-photo", ids[2]);
        await Expect(stage.Locator("[data-lightbox-original]")).ToHaveAttributeAsync("src", paths[2]);
        Assert.All(originals, path => Assert.Contains(path, new[] { paths[1], paths[2], paths[3] }));
        oldUrl = page.Url;
        await page.Keyboard.PressAsync("ArrowRight");
        await page.WaitForURLAsync(url => url != oldUrl);
        await Expect(stage).ToHaveAttributeAsync("data-lightbox-photo", ids[3]);
        await Expect(stage).ToHaveAttributeAsync("data-state", "loading");
        await page.Keyboard.PressAsync("Escape");
        await Expect(stage).ToHaveCountAsync(0);
        await page.EvaluateAsync("async path => { window.lightboxDecodes[path](); await new Promise(requestAnimationFrame); await new Promise(requestAnimationFrame); }", paths[3]);
        Assert.Equal(3, originals.Count);
        await page.GetByRole(AriaRole.Button, new() { Name = "查看大图", Exact = true }).ClickAsync();
        await Expect(stage).ToHaveAttributeAsync("data-state", "ready");
        await Expect(stage.Locator("[data-lightbox-original]")).ToHaveCSSAsync("animation-name", "none");
        Assert.Equal(1, originals.Count(path => path == paths[3]));
        await page.EvaluateAsync("() => Object.values(window.lightboxDecodes).forEach(resolve => resolve())");
        Assert.Empty(errors);
    }

    [Fact]
    public async Task Reduced_motion_disables_the_loading_spin_and_original_fade() {
        if (!E2EBaseUrl.TryGet(out var baseUrl)) return;
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchAsync(playwright);
        var page = await NewPageAsync(browser);
        await page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        var originals = NewGate();
        var previews = NewGate();
        string? firstPath = null;
        var delayThumbnails = false;
        await page.RouteAsync("**/media/photography/**", async route => {
            var path = new Uri(route.Request.Url).AbsolutePath;
            firstPath ??= path;
            if (path != firstPath) await originals.Task;
            if (!page.IsClosed) await route.FallbackAsync();
        });
        await page.RouteAsync("**/api/v1/photos/*/thumbnail?*", async route => {
            if (delayThumbnails) await previews.Task;
            if (!page.IsClosed) await route.FallbackAsync();
        });
        try {
            await OpenLightboxAsync(page, baseUrl, 8);
            var stage = page.Locator("[data-lightbox-stage]");
            await Expect(stage).ToHaveAttributeAsync("data-state", "ready");
            delayThumbnails = true;
            var url = page.Url;
            await page.Keyboard.PressAsync("ArrowRight");
            await page.WaitForURLAsync(current => current != url);
            var spinner = stage.Locator("[data-lightbox-loading]");
            await Expect(spinner).ToBeVisibleAsync();
            await Expect(spinner).ToHaveCSSAsync("background-color", "rgb(0, 0, 0)");
            await Expect(spinner.Locator("svg")).ToHaveCSSAsync("animation-name", "none");
            previews.TrySetResult();
            originals.TrySetResult();
            await Expect(stage).ToHaveAttributeAsync("data-state", "ready");
            await Expect(stage.Locator("[data-lightbox-original]")).ToHaveCSSAsync("animation-name", "none");
        } finally {
            previews.TrySetResult();
            originals.TrySetResult();
        }
    }
}
