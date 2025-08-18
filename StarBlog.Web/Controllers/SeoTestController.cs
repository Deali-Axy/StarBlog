using Microsoft.AspNetCore.Mvc;
using StarBlog.Web.Services;
using FreeSql;
using StarBlog.Data.Models;

namespace StarBlog.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class SeoTestController : Controller {
    private readonly SeoService _seoService;
    private readonly StructuredDataService _structuredDataService;
    private readonly ImageSeoService _imageSeoService;
    private readonly SitemapService _sitemapService;
    private readonly IBaseRepository<Post> _postRepo;

    public SeoTestController(
        SeoService seoService,
        StructuredDataService structuredDataService,
        ImageSeoService imageSeoService,
        SitemapService sitemapService,
        IBaseRepository<Post> postRepo) {
        _seoService = seoService;
        _structuredDataService = structuredDataService;
        _imageSeoService = imageSeoService;
        _sitemapService = sitemapService;
        _postRepo = postRepo;
    }

    [HttpGet("seo-test")]
    public async Task<IActionResult> Index() {
        var testResults = new List<string>();

        try {
            // 测试SEO服务
            var homeSeo = _seoService.GetHomeSeoMetadata();
            testResults.Add($"✅ 首页SEO元数据生成成功: {homeSeo.Title}");

            // 测试结构化数据服务
            var websiteStructuredData = _structuredDataService.GetWebsiteStructuredData();
            testResults.Add($"✅ 网站结构化数据生成成功: {websiteStructuredData.Length} 字符");

            var organizationStructuredData = _structuredDataService.GetOrganizationStructuredData();
            testResults.Add($"✅ 组织结构化数据生成成功: {organizationStructuredData.Length} 字符");

            var personStructuredData = _structuredDataService.GetPersonStructuredData();
            testResults.Add($"✅ 个人结构化数据生成成功: {personStructuredData.Length} 字符");

            // 测试文章相关功能
            var firstPost = await _postRepo.Where(p => p.IsPublish).FirstAsync();
            if (firstPost != null) {
                // 测试图片SEO优化
                var testMarkdown = "![测试图片](test-image.jpg)";
                var optimizedMarkdown = _imageSeoService.OptimizeImagesInMarkdown(testMarkdown, firstPost.Title);
                testResults.Add($"✅ Markdown图片SEO优化成功: {optimizedMarkdown}");

                var testHtml = "<img src='test.jpg'>";
                var optimizedHtml = _imageSeoService.OptimizeImagesInHtml(testHtml, firstPost.Title);
                testResults.Add($"✅ HTML图片SEO优化成功: {optimizedHtml}");

                // 测试图片URL提取
                var imageUrls = _imageSeoService.ExtractImageUrls(firstPost);
                testResults.Add($"✅ 图片URL提取成功: 找到 {imageUrls.Count} 张图片");
            }

            // 测试Sitemap服务
            var mainSitemap = await _sitemapService.GenerateMainSitemapAsync();
            testResults.Add($"✅ 主sitemap生成成功: {mainSitemap.Length} 字符");

            var imageSitemap = await _sitemapService.GenerateImageSitemapAsync();
            testResults.Add($"✅ 图片sitemap生成成功: {imageSitemap.Length} 字符");

            var sitemapIndex = _sitemapService.GenerateSitemapIndex();
            testResults.Add($"✅ Sitemap索引生成成功: {sitemapIndex.Length} 字符");

            testResults.Add("🎉 所有SEO功能测试通过！");
        }
        catch (Exception ex) {
            testResults.Add($"❌ 测试失败: {ex.Message}");
        }

        ViewBag.TestResults = testResults;
        return View();
    }

    [HttpGet("seo-test/sitemap")]
    public IActionResult TestSitemap() {
        var sitemapUrls = new List<string> {
            "/sitemap.xml",
            "/sitemap-images.xml",
            "/sitemap-index.xml"
        };

        ViewBag.SitemapUrls = sitemapUrls;
        return View();
    }

    [HttpGet("seo-test/meta")]
    public async Task<IActionResult> TestMeta() {
        var posts = _postRepo.Where(a => a.IsPublish).ToList();
        if (posts.Count == 0) {
            ViewBag.Error = "没有找到已发布的文章";
            return View();
        }
        
        var post = posts[Random.Shared.Next(posts.Count)];

        // 生成测试用的PostViewModel
        var postViewModel = new StarBlog.Web.ViewModels.PostViewModel {
            Id = post.Id,
            Title = post.Title,
            Summary = post.Summary ?? "测试摘要",
            Content = post.Content ?? "测试内容",
            CreationTime = post.CreationTime,
            LastUpdateTime = post.LastUpdateTime,
            Category = post.Category ?? new Category(),
            Categories = new List<Category>()
        };

        // 设置SEO元数据
        ViewData["SeoMetadata"] = _seoService.GetPostSeoMetadata(postViewModel);

        // 设置结构化数据
        var structuredData = new Dictionary<string, string> {
            ["BlogPosting"] = _structuredDataService.GetBlogPostingStructuredData(postViewModel),
            ["BreadcrumbList"] = _structuredDataService.GetBreadcrumbStructuredData(postViewModel),
            ["Person"] = _structuredDataService.GetPersonStructuredData()
        };
        ViewData["StructuredData"] = structuredData;

        return View(postViewModel);
    }
}
