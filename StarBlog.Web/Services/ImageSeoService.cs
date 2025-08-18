using System.Text.RegularExpressions;
using StarBlog.Data.Models;

namespace StarBlog.Web.Services;

public class ImageSeoService {
    private readonly ConfigService _configService;

    public ImageSeoService(ConfigService configService) {
        _configService = configService;
    }

    /// <summary>
    /// 为Markdown内容中的图片添加SEO优化的alt属性
    /// </summary>
    public string OptimizeImagesInMarkdown(string content, string postTitle) {
        if (string.IsNullOrEmpty(content)) return content;

        // 匹配Markdown图片语法: ![alt](url "title")
        var imagePattern = @"!\[([^\]]*)\]\(([^)]+)(?:\s+""([^""]*)"")?\)";
        
        return Regex.Replace(content, imagePattern, match => {
            var altText = match.Groups[1].Value;
            var imageUrl = match.Groups[2].Value;
            var title = match.Groups[3].Value;

            // 如果没有alt文本，生成一个SEO友好的alt
            if (string.IsNullOrEmpty(altText)) {
                altText = GenerateAltText(imageUrl, postTitle);
            }

            // 如果没有title，使用alt作为title
            if (string.IsNullOrEmpty(title)) {
                title = altText;
            }

            return $"![{altText}]({imageUrl} \"{title}\")";
        });
    }

    /// <summary>
    /// 为HTML内容中的图片添加SEO优化
    /// </summary>
    public string OptimizeImagesInHtml(string htmlContent, string postTitle) {
        if (string.IsNullOrEmpty(htmlContent)) return htmlContent;

        // 匹配HTML img标签
        var imgPattern = @"<img\s+([^>]*?)>";
        
        return Regex.Replace(htmlContent, imgPattern, match => {
            var imgTag = match.Value;
            var attributes = match.Groups[1].Value;

            // 检查是否已有alt属性
            if (!attributes.Contains("alt=")) {
                var srcMatch = Regex.Match(attributes, @"src=[""']([^""']+)[""']");
                if (srcMatch.Success) {
                    var imageUrl = srcMatch.Groups[1].Value;
                    var altText = GenerateAltText(imageUrl, postTitle);
                    attributes += $" alt=\"{altText}\"";
                }
            }

            // 添加loading="lazy"以提升性能
            if (!attributes.Contains("loading=")) {
                attributes += " loading=\"lazy\"";
            }

            // 添加decoding="async"以提升性能
            if (!attributes.Contains("decoding=")) {
                attributes += " decoding=\"async\"";
            }

            return $"<img {attributes}>";
        });
    }

    /// <summary>
    /// 生成SEO友好的alt文本
    /// </summary>
    private string GenerateAltText(string imageUrl, string postTitle) {
        // 从URL中提取文件名
        var fileName = Path.GetFileNameWithoutExtension(imageUrl);
        
        // 清理文件名，移除特殊字符
        fileName = Regex.Replace(fileName, @"[^a-zA-Z0-9\u4e00-\u9fa5\s-]", "");
        
        // 如果文件名有意义，使用它；否则使用文章标题
        if (!string.IsNullOrEmpty(fileName) && fileName.Length > 3 && !IsGuidLike(fileName)) {
            return $"{fileName} - {postTitle}";
        }

        return $"{postTitle} 相关图片";
    }

    /// <summary>
    /// 检查字符串是否像GUID
    /// </summary>
    private bool IsGuidLike(string str) {
        return str.Length >= 16 && Regex.IsMatch(str, @"^[a-fA-F0-9-]+$");
    }

    /// <summary>
    /// 获取文章中的所有图片URL
    /// </summary>
    public List<string> ExtractImageUrls(Post post) {
        var imageUrls = new List<string>();
        var baseUrl = GetBaseUrl();

        if (string.IsNullOrEmpty(post.Content)) return imageUrls;

        // 从Markdown内容中提取图片
        var imagePattern = @"!\[([^\]]*)\]\(([^)]+)(?:\s+""([^""]*)"")?\)";
        var matches = Regex.Matches(post.Content, imagePattern);

        foreach (Match match in matches) {
            var imageUrl = match.Groups[2].Value;
            
            // 转换为绝对URL
            if (!imageUrl.StartsWith("http")) {
                if (imageUrl.StartsWith("/")) {
                    imageUrl = baseUrl + imageUrl;
                } else {
                    imageUrl = $"{baseUrl}/media/blog/{post.Id}/{imageUrl}";
                }
            }

            imageUrls.Add(imageUrl);
        }

        return imageUrls;
    }

    /// <summary>
    /// 生成图片sitemap XML
    /// </summary>
    public async Task<string> GenerateImageSitemap(IEnumerable<Post> posts) {
        var baseUrl = GetBaseUrl();
        var xml = new System.Text.StringBuilder();
        
        xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        xml.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"");
        xml.AppendLine("        xmlns:image=\"http://www.google.com/schemas/sitemap-image/1.1\">");

        foreach (var post in posts) {
            var imageUrls = ExtractImageUrls(post);
            if (!imageUrls.Any()) continue;

            var postUrl = string.IsNullOrWhiteSpace(post.Slug) 
                ? $"{baseUrl}/Blog/Post/{post.Id}"
                : $"{baseUrl}/p/{post.Slug}";

            xml.AppendLine("  <url>");
            xml.AppendLine($"    <loc>{postUrl}</loc>");

            foreach (var imageUrl in imageUrls) {
                xml.AppendLine("    <image:image>");
                xml.AppendLine($"      <image:loc>{imageUrl}</image:loc>");
                xml.AppendLine($"      <image:caption>{post.Title} 相关图片</image:caption>");
                xml.AppendLine($"      <image:title>{post.Title}</image:title>");
                xml.AppendLine("    </image:image>");
            }

            xml.AppendLine("  </url>");
        }

        xml.AppendLine("</urlset>");
        return xml.ToString();
    }

    private string GetBaseUrl() {
        var host = _configService["host"];
        return !string.IsNullOrEmpty(host) ? host.TrimEnd('/') : "https://blog.deali.cn";
    }
}
