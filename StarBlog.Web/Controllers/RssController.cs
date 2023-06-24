using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;

namespace StarBlog.Web.Apis.Blog;

[ApiController]
[Route("feed")]
[ApiExplorerSettings(IgnoreApi = true)]
public class RssController : ControllerBase {
    private readonly IBaseRepository<Post> _postRepo;

    public RssController(IBaseRepository<Post> postRepo) {
        _postRepo = postRepo;
    }

    [ResponseCache(Duration = 1200)]
    [HttpGet]
    public async Task<IActionResult> Index() {
        var posts = await _postRepo.Where(a => a.IsPublish && a.CreationTime.Year == DateTime.Now.Year)
            .OrderByDescending(a => a.LastUpdateTime)
            .Include(a => a.Category)
            .ToListAsync();
        
        var feed = new SyndicationFeed(
            "StarBlog",
            "「程序设计实验室」 专注于互联网热门新技术探索与团队敏捷开发实践，包括架构设计、机器学习与数据分析算法、移动端开发、Linux、Web前后端开发等，欢迎一起探讨技术，分享学习实践经验。",
            new Uri("http://blog.deali.cn"), "RSSUrl", posts.First().LastUpdateTime
        ) {
            Copyright = new TextSyndicationContent($"{DateTime.Now.Year} DealiAxy")
        };

        var items = new List<SyndicationItem>();
        foreach (var item in posts) {
            var postUrl = Url.Action("Post", "Blog", new {id = item.Id}, HttpContext.Request.Scheme);
            items.Add(new SyndicationItem(item.Title,
                new TextSyndicationContent(PostService.GetContentHtml(item), TextSyndicationContentKind.Html),
                new Uri(postUrl), item.Id, item.LastUpdateTime
            ) {
                Categories = {new SyndicationCategory(item.Category?.Name)},
                Authors = {new SyndicationPerson("admin@deali.cn", "DealiAxy", "https://deali.cn")},
                PublishDate = item.CreationTime,
                Summary = new TextSyndicationContent(item.Summary)
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
        var rssFormatter = new Atom10FeedFormatter(feed);
        rssFormatter.WriteTo(xmlWriter);
        await xmlWriter.FlushAsync();

        return File(stream.ToArray(), "application/xml; charset=utf-8");
    }
}