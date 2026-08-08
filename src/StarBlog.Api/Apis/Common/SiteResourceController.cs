using System.Text;
using System.Xml;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Api.Extensions;
using StarBlog.Application.Services;
using StarBlog.Content.Utils;
using StarBlog.Data.Models;

namespace StarBlog.Api.Apis.Common;

/// <summary>
/// 从旧 MVC 项目迁移的可索引站点资源（RSS 与 Sitemap）。
/// 这些端点不是 JSON，但仍由纯 API 项目托管，以保持既有搜索引擎地址兼容。
/// </summary>
[ApiController]
[Route("")]
[ApiExplorerSettings(GroupName = ApiGroups.Common)]
public sealed class SiteResourceController : ControllerBase {
    private readonly IBaseRepository<Post> _postRepository;
    private readonly IBaseRepository<Category> _categoryRepository;
    private readonly ConfigService _configService;

    public SiteResourceController(
        IBaseRepository<Post> postRepository,
        IBaseRepository<Category> categoryRepository,
        ConfigService configService) {
        _postRepository = postRepository;
        _categoryRepository = categoryRepository;
        _configService = configService;
    }

    /// <summary>生成 Atom 1.0 feed，保留旧项目的 <c>/feed</c> 地址。</summary>
    [AllowAnonymous]
    [HttpGet("feed")]
    [ResponseCache(Duration = 1200)]
    public async Task<IActionResult> Feed() {
        var posts = await _postRepository.Where(post => post.IsPublish && post.CreationTime.Year == DateTime.Now.Year)
            .OrderByDescending(post => post.LastUpdateTime)
            .ToListAsync();
        var baseUrl = GetBaseUrl();
        var updated = posts.FirstOrDefault()?.LastUpdateTime ?? DateTimeOffset.UtcNow;

        return Xml(await WriteXmlAsync(async writer => {
            await writer.WriteStartElementAsync(null, "feed", "http://www.w3.org/2005/Atom");
            await writer.WriteElementStringAsync(null, "title", null, "StarBlog");
            await writer.WriteElementStringAsync(null, "id", null, $"{baseUrl}/feed");
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
                await writer.WriteCDataAsync(PostService.GetContentHtml(post));
                await writer.WriteEndElementAsync();
                await writer.WriteElementStringAsync(null, "summary", null, post.Summary ?? string.Empty);
                await writer.WriteEndElementAsync();
            }

            await writer.WriteEndElementAsync();
        }), "application/atom+xml; charset=utf-8");
    }

    /// <summary>返回 robots.txt，并指向 API 项目生成的 sitemap。</summary>
    [AllowAnonymous]
    [HttpGet("robots.txt")]
    [ResponseCache(Duration = 3600)]
    public IActionResult Robots() {
        var baseUrl = GetBaseUrl();
        var content = $"""
            User-agent: *
            Disallow: /Api/Admin/
            Disallow: /api-docs/
            Allow: /
            Sitemap: {baseUrl}/sitemap.xml
            Sitemap: {baseUrl}/sitemap-images.xml
            """;
        return Content(content, "text/plain; charset=utf-8", Encoding.UTF8);
    }

    /// <summary>生成 sitemap 索引，保留搜索引擎已收录的固定地址。</summary>
    [AllowAnonymous]
    [HttpGet("sitemap-index.xml")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> SitemapIndex() {
        var baseUrl = GetBaseUrl();
        return Xml(await WriteXmlAsync(async writer => {
            await writer.WriteStartElementAsync(null, "sitemapindex", "http://www.sitemaps.org/schemas/sitemap/0.9");
            await WriteSitemapEntry(writer, $"{baseUrl}/sitemap.xml");
            await WriteSitemapEntry(writer, $"{baseUrl}/sitemap-images.xml");
            await writer.WriteEndElementAsync();
        }));
    }

    /// <summary>生成文章与分类的标准 sitemap。</summary>
    [AllowAnonymous]
    [HttpGet("sitemap.xml")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> Sitemap() {
        var baseUrl = GetBaseUrl();
        var categories = await _categoryRepository.Where(category => category.Visible).ToListAsync();
        var posts = await _postRepository.Where(post => post.IsPublish).OrderByDescending(post => post.LastUpdateTime).ToListAsync();

        return Xml(await WriteXmlAsync(async writer => {
            await writer.WriteStartElementAsync(null, "urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
            await WriteUrlEntry(writer, baseUrl, DateTime.UtcNow, "daily", "1.0");
            await WriteUrlEntry(writer, $"{baseUrl}/Blog/List", DateTime.UtcNow, "daily", "0.8");
            foreach (var category in categories) await WriteUrlEntry(writer, $"{baseUrl}/Blog/List?categoryId={category.Id}", DateTime.UtcNow, "weekly", "0.7");
            foreach (var post in posts) {
                var url = string.IsNullOrWhiteSpace(post.Slug) ? $"{baseUrl}/Blog/Post/{post.Id}" : $"{baseUrl}/p/{Uri.EscapeDataString(post.Slug)}";
                await WriteUrlEntry(writer, url, post.LastUpdateTime, "monthly", "0.6");
            }
            await writer.WriteEndElementAsync();
        }));
    }

    /// <summary>
    /// 图片 Sitemap 已迁移为兼容端点。图片由前端/存储域管理时，可在此基础上追加图片 URL。
    /// </summary>
    [AllowAnonymous]
    [HttpGet("sitemap-images.xml")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> ImageSitemap() {
        var posts = await _postRepository.Where(post => post.IsPublish).ToListAsync();
        var baseUrl = GetBaseUrl();
        return Xml(await WriteXmlAsync(async writer => {
            await writer.WriteStartElementAsync(null, "urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
            await writer.WriteAttributeStringAsync("xmlns", "image", null, "http://www.google.com/schemas/sitemap-image/1.1");
            foreach (var post in posts) {
                var images = ExtractImageUrls(post.Content ?? string.Empty);
                if (images.Count == 0) continue;
                var pageUrl = string.IsNullOrWhiteSpace(post.Slug) ? $"{baseUrl}/Blog/Post/{post.Id}" : $"{baseUrl}/p/{Uri.EscapeDataString(post.Slug)}";
                await writer.WriteStartElementAsync(null, "url", null);
                await writer.WriteElementStringAsync(null, "loc", null, pageUrl);
                foreach (var image in images) {
                    await writer.WriteStartElementAsync("image", "image", "http://www.google.com/schemas/sitemap-image/1.1");
                    await writer.WriteElementStringAsync("image", "loc", "http://www.google.com/schemas/sitemap-image/1.1", ToAbsoluteUrl(baseUrl, image));
                    await writer.WriteElementStringAsync("image", "title", "http://www.google.com/schemas/sitemap-image/1.1", post.Title);
                    await writer.WriteEndElementAsync();
                }
                await writer.WriteEndElementAsync();
            }
            await writer.WriteEndElementAsync();
        }));
    }

    private IActionResult Xml(byte[] data, string contentType = "application/xml; charset=utf-8") => File(data, contentType);

    private static async Task<byte[]> WriteXmlAsync(Func<XmlWriter, Task> write) {
        var settings = new XmlWriterSettings { Async = true, Encoding = new UTF8Encoding(false), Indent = true };
        await using var stream = new MemoryStream();
        await using (var writer = XmlWriter.Create(stream, settings)) {
            await write(writer);
            await writer.FlushAsync();
        }
        return stream.ToArray();
    }

    private static async Task WriteSitemapEntry(XmlWriter writer, string location) {
        await writer.WriteStartElementAsync(null, "sitemap", null);
        await writer.WriteElementStringAsync(null, "loc", null, location);
        await writer.WriteElementStringAsync(null, "lastmod", null, DateTime.UtcNow.ToString("yyyy-MM-dd"));
        await writer.WriteEndElementAsync();
    }

    private static async Task WriteUrlEntry(XmlWriter writer, string location, DateTime lastModified, string changeFrequency, string priority) {
        await writer.WriteStartElementAsync(null, "url", null);
        await writer.WriteElementStringAsync(null, "loc", null, location);
        await writer.WriteElementStringAsync(null, "lastmod", null, lastModified.ToUniversalTime().ToString("yyyy-MM-dd"));
        await writer.WriteElementStringAsync(null, "changefreq", null, changeFrequency);
        await writer.WriteElementStringAsync(null, "priority", null, priority);
        await writer.WriteEndElementAsync();
    }

    private string GetBaseUrl() => (_configService["host"] ?? string.Empty).TrimEnd('/') is { Length: > 0 } host ? host : "https://blog.deali.cn";

    private static List<string> ExtractImageUrls(string markdown) => System.Text.RegularExpressions.Regex
        .Matches(markdown, @"!\[[^\]]*\]\((?<url>[^\s)]+)")
        .Select(match => match.Groups["url"].Value)
        .Where(url => !string.IsNullOrWhiteSpace(url))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    private static string ToAbsoluteUrl(string baseUrl, string url) => Uri.TryCreate(url, UriKind.Absolute, out _) ? url : $"{baseUrl}/{url.TrimStart('/')}";
}
