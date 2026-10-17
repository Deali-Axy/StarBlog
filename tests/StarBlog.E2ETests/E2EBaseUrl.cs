using Microsoft.Playwright;

namespace StarBlog.E2ETests;

internal static class E2EBaseUrl {
    public static async Task UseApiOverrideAsync(IPage page) {
        var apiUrl = Environment.GetEnvironmentVariable("E2E_API_BASE_URL");
        if (string.IsNullOrWhiteSpace(apiUrl)) return;

        // Production rewrites are compiled at build time. Keep every public API
        // resource on the isolated fixture instance, including robots and feeds.
        async Task ProxyAsync(IRoute route) {
            var path = new Uri(route.Request.Url).PathAndQuery;
            var response = await route.FetchAsync(new() { Url = $"{apiUrl.TrimEnd('/')}{path}" });
            await route.FulfillAsync(new() { Response = response });
        }
        foreach (var pattern in new[] { "**/api/**", "**/media/**", "**/robots.txt", "**/feed", "**/sitemap*.xml" }) {
            await page.RouteAsync(pattern, ProxyAsync);
        }
    }

    public static bool TryGet(out string baseUrl) {
        var baseUrlEnv = Environment.GetEnvironmentVariable("E2E_BASE_URL");
        if (string.IsNullOrWhiteSpace(baseUrlEnv)) {
            baseUrl = string.Empty;
            return false;
        }

        if (!Uri.TryCreate(baseUrlEnv, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)) {
            baseUrl = string.Empty;
            return false;
        }

        baseUrl = baseUrlEnv.TrimEnd('/');
        return true;
    }
}
