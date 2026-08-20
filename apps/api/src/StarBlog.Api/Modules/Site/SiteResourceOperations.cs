using System.Text;
using System.Xml;
using StarBlog.Api.Modules.Configuration;
using StarBlog.Api.Modules.Content;
using StarBlog.Api.Modules.Content.Markdown;

namespace StarBlog.Api.Modules.Site;

/// <summary>
/// Feed、robots.txt 与 sitemap。
/// </summary>
public sealed class SiteResourceOperations {
    private readonly IPublishedContentQueries _content;
    private readonly ISiteConfiguration _configuration;

    public SiteResourceOperations(IPublishedContentQueries content, ISiteConfiguration configuration) {
        _content = content;
        _configuration = configuration;
    }

    public async Task<string> GetBaseUrlAsync(CancellationToken cancellationToken) =>
        (await _configuration.GetAsync("host", cancellationToken))?.TrimEnd('/') ?? "https://blog.deali.cn";

    public async Task<byte[]> BuildFeedAsync(CancellationToken cancellationToken) {
        var posts = (await _content.GetPublishedAsync(cancellationToken))
            .Where(post => post.CreationTime.Year == DateTime.Now.Year)
            .ToList();
        var baseUrl = await GetBaseUrlAsync(cancellationToken);
        return await WriteXmlAsync(async writer => {
            await writer.WriteStartElementAsync(null, "feed", "http://www.w3.org/2005/Atom");
            await writer.WriteElementStringAsync(null, "title", null, "StarBlog");
            await writer.WriteElementStringAsync(null, "id", null, $"{baseUrl}/feed");
            var updated = posts.FirstOrDefault()?.LastUpdateTime ?? DateTimeOffset.UtcNow;
            await writer.WriteElementStringAsync(null, "updated", null, updated.ToUniversalTime().ToString("O"));
            await writer.WriteStartElementAsync(null, "link", null);
            await writer.WriteAttributeStringAsync(null, "href", null, $"{baseUrl}/feed");
            await writer.WriteEndElementAsync();
            foreach (var post in posts) {
                var postUrl = string.IsNullOrWhiteSpace(post.Slug) ? $"{baseUrl}/Blog/Post/{post.Id}" : $"{baseUrl}/p/{Uri.EscapeDataString(post.Slug)}";
                await writer.WriteStartElementAsync(null, "entry", null);
                await writer.WriteElementStringAsync(null, "title", null, post.Title);
                await writer.WriteElementStringAsync(null, "id", null, postUrl);
                await writer.WriteElementStringAsync(null, "updated", null, post.LastUpdateTime.ToUniversalTime().ToString("O"));
                await writer.WriteElementStringAsync(null, "published", null, post.CreationTime.ToUniversalTime().ToString("O"));
                await writer.WriteStartElementAsync(null, "link", null);
                await writer.WriteAttributeStringAsync(null, "href", null, postUrl);
                await writer.WriteEndElementAsync();
                await writer.WriteStartElementAsync(null, "content", null);
                await writer.WriteAttributeStringAsync(null, "type", null, "html");
                await writer.WriteCDataAsync(MarkdownRenderer.ToHtml(post.Content));
                await writer.WriteEndElementAsync();
                await writer.WriteElementStringAsync(null, "summary", null, post.Summary ?? string.Empty);
                await writer.WriteEndElementAsync();
            }

            await writer.WriteEndElementAsync();
        });
    }

    public async Task<string> BuildRobotsAsync(CancellationToken cancellationToken) {
        var baseUrl = await GetBaseUrlAsync(cancellationToken);
        return $"""
            User-agent: *
            Disallow: /api/v1/admin/
            Disallow: /api-docs/
            Allow: /
            Sitemap: {baseUrl}/sitemap.xml
            Sitemap: {baseUrl}/sitemap-images.xml
            """;
    }

    public async Task<byte[]> BuildSitemapIndexAsync(CancellationToken cancellationToken) {
        var baseUrl = await GetBaseUrlAsync(cancellationToken);
        return await WriteXmlAsync(async writer => {
            await writer.WriteStartElementAsync(null, "sitemapindex", "http://www.sitemaps.org/schemas/sitemap/0.9");
            await WriteSitemapEntry(writer, $"{baseUrl}/sitemap.xml");
            await WriteSitemapEntry(writer, $"{baseUrl}/sitemap-images.xml");
            await writer.WriteEndElementAsync();
        });
    }

    public async Task<byte[]> BuildSitemapAsync(CancellationToken cancellationToken) {
        var baseUrl = await GetBaseUrlAsync(cancellationToken);
        var categories = await _content.GetVisibleCategoriesAsync(cancellationToken);
        var posts = await _content.GetPublishedAsync(cancellationToken);
        return await WriteXmlAsync(async writer => {
            await writer.WriteStartElementAsync(null, "urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
            await WriteUrl(writer, baseUrl, DateTime.UtcNow, "daily", "1.0");
            await WriteUrl(writer, $"{baseUrl}/Blog/List", DateTime.UtcNow, "daily", "0.8");
            foreach (var category in categories) {
                await WriteUrl(writer, $"{baseUrl}/Blog/List?categoryId={category.Id}", DateTime.UtcNow, "weekly", "0.7");
            }

            foreach (var post in posts) {
                var url = string.IsNullOrWhiteSpace(post.Slug) ? $"{baseUrl}/Blog/Post/{post.Id}" : $"{baseUrl}/p/{Uri.EscapeDataString(post.Slug)}";
                await WriteUrl(writer, url, post.LastUpdateTime.ToUniversalTime(), "weekly", "0.6");
            }

            await writer.WriteEndElementAsync();
        });
    }

    public async Task<byte[]> BuildImageSitemapAsync(CancellationToken cancellationToken) {
        var baseUrl = await GetBaseUrlAsync(cancellationToken);
        return await WriteXmlAsync(async writer => {
            await writer.WriteStartElementAsync(null, "urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
            await writer.WriteAttributeStringAsync("xmlns", "image", null, "http://www.google.com/schemas/sitemap-image/1.1");
            await WriteUrl(writer, $"{baseUrl}/Photography", DateTime.UtcNow, "weekly", "0.5");
            await writer.WriteEndElementAsync();
        });
    }

    private static async Task WriteSitemapEntry(XmlWriter writer, string loc) {
        await writer.WriteStartElementAsync(null, "sitemap", null);
        await writer.WriteElementStringAsync(null, "loc", null, loc);
        await writer.WriteEndElementAsync();
    }

    private static async Task WriteUrl(XmlWriter writer, string loc, DateTime lastmod, string changefreq, string priority) {
        await writer.WriteStartElementAsync(null, "url", null);
        await writer.WriteElementStringAsync(null, "loc", null, loc);
        await writer.WriteElementStringAsync(null, "lastmod", null, lastmod.ToString("O"));
        await writer.WriteElementStringAsync(null, "changefreq", null, changefreq);
        await writer.WriteElementStringAsync(null, "priority", null, priority);
        await writer.WriteEndElementAsync();
    }

    private static async Task<byte[]> WriteXmlAsync(Func<XmlWriter, Task> write) {
        await using var stream = new MemoryStream();
        var settings = new XmlWriterSettings { Async = true, Encoding = new UTF8Encoding(false), Indent = true };
        await using (var writer = XmlWriter.Create(stream, settings)) {
            await writer.WriteStartDocumentAsync();
            await write(writer);
            await writer.WriteEndDocumentAsync();
        }

        return stream.ToArray();
    }
}
