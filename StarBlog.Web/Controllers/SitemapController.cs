using System.Text;
using System.Xml;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;

namespace StarBlog.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class SitemapController : Controller {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly ConfigService _configService;
    private readonly ILogger<SitemapController> _logger;
    private readonly ImageSeoService _imageSeoService;

    public SitemapController(
        IBaseRepository<Post> postRepo,
        IBaseRepository<Category> categoryRepo,
        ConfigService configService,
        ILogger<SitemapController> logger,
        ImageSeoService imageSeoService) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _configService = configService;
        _logger = logger;
        _imageSeoService = imageSeoService;
    }

    [HttpGet("sitemap-index.xml")]
    [ResponseCache(Duration = 3600)] // 缓存1小时
    public IActionResult SitemapIndex() {
        try {
            var baseUrl = GetBaseUrl();
            var sitemapIndex = GenerateSitemapIndex(baseUrl);
            return Content(sitemapIndex, "application/xml", Encoding.UTF8);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "生成sitemap索引时发生错误");
            return StatusCode(500);
        }
    }

    [HttpGet("sitemap.xml")]
    [ResponseCache(Duration = 3600)] // 缓存1小时
    public async Task<IActionResult> Index() {
        try {
            var baseUrl = GetBaseUrl();
            var sitemap = await GenerateSitemap(baseUrl);
            return Content(sitemap, "application/xml", Encoding.UTF8);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "生成sitemap时发生错误");
            return StatusCode(500);
        }
    }

    private async Task<string> GenerateSitemap(string baseUrl) {
        var settings = new XmlWriterSettings {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

        // 添加首页
        WriteUrl(xmlWriter, baseUrl, DateTime.Now, "daily", "1.0");

        // 添加博客列表页
        WriteUrl(xmlWriter, $"{baseUrl}/Blog/List", DateTime.Now, "daily", "0.8");

        // 添加分类页面
        var categories = await _categoryRepo.Where(c => c.Visible).ToListAsync();
        foreach (var category in categories) {
            var categoryUrl = $"{baseUrl}/Blog/List?categoryId={category.Id}";
            WriteUrl(xmlWriter, categoryUrl, DateTime.Now, "weekly", "0.7");
        }

        // 添加文章页面
        var posts = await _postRepo.Where(p => p.IsPublish)
            .OrderByDescending(p => p.LastUpdateTime)
            .ToListAsync();

        foreach (var post in posts) {
            var postUrl = string.IsNullOrWhiteSpace(post.Slug)
                ? $"{baseUrl}/Blog/Post/{post.Id}"
                : $"{baseUrl}/p/{post.Slug}";

            WriteUrl(xmlWriter, postUrl, post.LastUpdateTime, "monthly", "0.6");
        }

        // 添加其他静态页面
        WriteUrl(xmlWriter, $"{baseUrl}/About", DateTime.Now, "monthly", "0.5");
        WriteUrl(xmlWriter, $"{baseUrl}/Photography", DateTime.Now, "weekly", "0.5");
        WriteUrl(xmlWriter, $"{baseUrl}/LinkExchange", DateTime.Now, "monthly", "0.4");

        xmlWriter.WriteEndElement(); // urlset
        xmlWriter.WriteEndDocument();

        return stringWriter.ToString();
    }

    private static void WriteUrl(XmlWriter xmlWriter, string url, DateTime lastModified,
        string changeFreq, string priority) {
        xmlWriter.WriteStartElement("url");
        xmlWriter.WriteElementString("loc", url);
        xmlWriter.WriteElementString("lastmod", lastModified.ToString("yyyy-MM-ddTHH:mm:sszzz"));
        xmlWriter.WriteElementString("changefreq", changeFreq);
        xmlWriter.WriteElementString("priority", priority);
        xmlWriter.WriteEndElement();
    }

    private string GetBaseUrl() {
        var host = _configService["host"];
        if (!string.IsNullOrEmpty(host)) {
            return host.TrimEnd('/');
        }

        var request = HttpContext.Request;
        return $"{request.Scheme}://{request.Host}";
    }

    [HttpGet("sitemap-images.xml")]
    [ResponseCache(Duration = 3600)] // 缓存1小时
    public async Task<IActionResult> ImageSitemap() {
        try {
            var posts = await _postRepo.Where(p => p.IsPublish).ToListAsync();
            var imageSitemap = await _imageSeoService.GenerateImageSitemap(posts);
            return Content(imageSitemap, "application/xml", Encoding.UTF8);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "生成图片sitemap时发生错误");
            return StatusCode(500);
        }
    }

    private string GenerateSitemapIndex(string baseUrl) {
        var settings = new XmlWriterSettings {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("sitemapindex", "http://www.sitemaps.org/schemas/sitemap/0.9");

        // 主sitemap
        WriteSitemapEntry(xmlWriter, $"{baseUrl}/sitemap.xml", DateTime.Now);

        // 图片sitemap
        WriteSitemapEntry(xmlWriter, $"{baseUrl}/sitemap-images.xml", DateTime.Now);

        xmlWriter.WriteEndElement(); // sitemapindex
        xmlWriter.WriteEndDocument();

        return stringWriter.ToString();
    }

    private static void WriteSitemapEntry(XmlWriter xmlWriter, string url, DateTime lastModified) {
        xmlWriter.WriteStartElement("sitemap");
        xmlWriter.WriteElementString("loc", url);
        xmlWriter.WriteElementString("lastmod", lastModified.ToString("yyyy-MM-ddTHH:mm:sszzz"));
        xmlWriter.WriteEndElement();
    }
}