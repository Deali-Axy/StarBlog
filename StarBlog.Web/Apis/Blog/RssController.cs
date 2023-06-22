using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;

namespace StarBlog.Web.Apis.Blog;

[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
public class RssController : ControllerBase {
    private readonly IBaseRepository<Post> _postRepo;

    public RssController(IBaseRepository<Post> postRepo) {
        _postRepo = postRepo;
    }

    [ResponseCache(Duration = 1200)]
    [HttpGet]
    public async Task<IActionResult> Index() {
        var feed = new SyndicationFeed(
            "StarBlog",
            "「程序设计实验室」 专注于互联网热门新技术探索与团队敏捷开发实践，包括架构设计、机器学习与数据分析算法、移动端开发、Linux、Web前后端开发等，欢迎一起探讨技术，分享学习实践经验。",
            new Uri("http://blog.deali.cn"), "RSSUrl", DateTime.Now
        ) {
            Copyright = new TextSyndicationContent($"{DateTime.Now.Year} DealiAxy")
        };

        var items = new List<SyndicationItem>();
        var posts = await _postRepo.Where(a => a.IsPublish)
            .Include(a => a.Category)
            .ToListAsync();
        foreach (var item in posts) {
            var postUrl = Url.Action("Post", "Blog", new { id = item.Id }, HttpContext.Request.Scheme);
            items.Add(new SyndicationItem(item.Title, item.Summary, new Uri(postUrl), item.Id, item.LastUpdateTime) {
                Categories = { new SyndicationCategory(item.Category?.Name) },
                Authors = { new SyndicationPerson("admin@deali.cn", "DealiAxy", "https://deali.cn") },
                PublishDate = item.CreationTime
            });
        }

        feed.Items = items;

        var settings = new XmlWriterSettings {
            Async = true,
            Encoding = Encoding.UTF8,
            NewLineHandling = NewLineHandling.Entitize,
            NewLineOnAttributes = true,
            Indent = true
        };
        using var stream = new MemoryStream();
        await using var xmlWriter = XmlWriter.Create(stream, settings);
        var rssFormatter = new Rss20FeedFormatter(feed, false);
        rssFormatter.WriteTo(xmlWriter);
        await xmlWriter.FlushAsync();

        return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
    }
}