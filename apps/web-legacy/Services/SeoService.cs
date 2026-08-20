using StarBlog.Data.Models;
using StarBlog.Web.ViewModels;

namespace StarBlog.Web.Services;

public class SeoService {
    private readonly ConfigService _configService;

    public SeoService(ConfigService configService) {
        _configService = configService;
    }

    public SeoMetadata GetHomeSeoMetadata() {
        var siteName = "曦远博客 · StarBlog · 画星星高手 · 程序设计实验室";
        var description =
            "DealiAxy，曦远博客，画星星高手博客，程序设计实验室。专注于互联网热门新技术探索与团队敏捷开发实践，包括架构设计、机器学习与数据分析算法、移动端开发、Linux、Web前后端开发等技术分享。";
        var keywords = "曦远,DealiAxy,程序设计实验室,博客,编程,技术,开发,StarBlog,画星星高手,程序设计,Web开发,机器学习,数据分析";

        return new SeoMetadata {
            Title = siteName,
            Description = description,
            Keywords = keywords,
            OgTitle = siteName,
            OgDescription = description,
            OgType = "website",
            OgUrl = GetBaseUrl(),
            TwitterCard = "summary_large_image",
            TwitterTitle = siteName,
            TwitterDescription = description
        };
    }

    public SeoMetadata GetPostSeoMetadata(PostViewModel post) {
        var title = $"{post.Title} - StarBlog";
        var description = !string.IsNullOrEmpty(post.Summary) && post.Summary != "（没有介绍）"
            ? post.Summary
            : ExtractDescriptionFromContent(post.Content);

        var keywords = GenerateKeywordsFromPost(post);
        var url = post.Url ?? $"{GetBaseUrl()}/Blog/Post/{post.Id}";

        return new SeoMetadata {
            Title = title,
            Description = description,
            Keywords = keywords,
            OgTitle = post.Title,
            OgDescription = description,
            OgType = "article",
            OgUrl = url,
            OgArticleAuthor = "DealiAxy",
            OgArticlePublishedTime = post.CreationTime,
            OgArticleModifiedTime = post.LastUpdateTime,
            OgArticleSection = post.Category?.Name,
            TwitterCard = "summary_large_image",
            TwitterTitle = post.Title,
            TwitterDescription = description,
            CanonicalUrl = url
        };
    }

    public SeoMetadata GetCategorySeoMetadata(Category category, int postCount) {
        var title = $"{category.Name} - 分类 - StarBlog";
        var description = $"StarBlog {category.Name} 分类下的所有文章，共 {postCount} 篇文章。";
        var keywords = $"{category.Name},分类,StarBlog,博客,技术文章";
        var url = $"{GetBaseUrl()}/Blog/List?categoryId={category.Id}";

        return new SeoMetadata {
            Title = title,
            Description = description,
            Keywords = keywords,
            OgTitle = title,
            OgDescription = description,
            OgType = "website",
            OgUrl = url,
            TwitterCard = "summary",
            TwitterTitle = title,
            TwitterDescription = description,
            CanonicalUrl = url
        };
    }

    public SeoMetadata GetBlogListSeoMetadata(int page = 1) {
        var title = page > 1 ? $"博客文章 - 第{page}页 - StarBlog" : "博客文章 - StarBlog";
        var description = "StarBlog 博客文章列表，分享编程技术、开发经验、技术教程等内容。";
        var keywords = "博客,文章列表,编程,技术,开发,StarBlog";
        var url = page > 1 ? $"{GetBaseUrl()}/Blog/List?page={page}" : $"{GetBaseUrl()}/Blog/List";

        return new SeoMetadata {
            Title = title,
            Description = description,
            Keywords = keywords,
            OgTitle = title,
            OgDescription = description,
            OgType = "website",
            OgUrl = url,
            TwitterCard = "summary",
            TwitterTitle = title,
            TwitterDescription = description,
            CanonicalUrl = url
        };
    }

    private string ExtractDescriptionFromContent(string content) {
        if (string.IsNullOrEmpty(content)) return "StarBlog 技术博客文章";

        // 移除Markdown标记
        var plainText = System.Text.RegularExpressions.Regex.Replace(content, @"[#*`\[\]()_~]", "");
        plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"!\[.*?\]\(.*?\)", "");
        plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"\[.*?\]\(.*?\)", "");

        // 取前160个字符作为描述
        return plainText.Length > 160 ? plainText.Substring(0, 157) + "..." : plainText;
    }

    private string GenerateKeywordsFromPost(PostViewModel post) {
        var keywords = new List<string> { "曦远", "DealiAxy", "StarBlog", "博客", "技术" };

        keywords.Add(post.Category.Name);

        // 从标题中提取关键词
        var titleWords = post.Title.Split(new[] { ' ', '-', '_', ':', '：', '，', '。' },
            StringSplitOptions.RemoveEmptyEntries);
        keywords.AddRange(titleWords.Where(w => w.Length > 1).Take(3));

        return string.Join(",", keywords.Distinct());
    }

    private string GetBaseUrl() {
        var host = _configService["host"];
        return !string.IsNullOrEmpty(host) ? host.TrimEnd('/') : "https://blog.deali.cn";
    }
}

public class SeoMetadata {
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Keywords { get; set; } = "";
    public string? CanonicalUrl { get; set; }

    // Open Graph
    public string OgTitle { get; set; } = "";
    public string OgDescription { get; set; } = "";
    public string OgType { get; set; } = "website";
    public string OgUrl { get; set; } = "";
    public string? OgImage { get; set; }
    public string? OgArticleAuthor { get; set; }
    public DateTime? OgArticlePublishedTime { get; set; }
    public DateTime? OgArticleModifiedTime { get; set; }
    public string? OgArticleSection { get; set; }

    // Twitter Card
    public string TwitterCard { get; set; } = "summary";
    public string TwitterTitle { get; set; } = "";
    public string TwitterDescription { get; set; } = "";
    public string? TwitterImage { get; set; }
}