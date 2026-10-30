using Microsoft.Playwright;

namespace StarBlog.E2ETests;

internal static class BlogPreview {
    public static async Task SelectAsync(IPage page, string label) {
        await page.Locator("[data-blog-preview-trigger]").ClickAsync();
        await page.GetByRole(AriaRole.Menuitemradio, new() { Name = label, Exact = true }).ClickAsync();
    }
}
