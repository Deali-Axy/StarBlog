using System.Text.RegularExpressions;
using System.Web;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using StarBlog.Content.Extensions;
using StarBlog.Content.Utils;
using StarBlog.Data.Models;

namespace StarBlog.Content;

public class PostProcessor {
    private readonly Post _post;
    private readonly string _importPath;
    private readonly string _assetsPath;

    public PostProcessor(string importPath, string assetsPath, Post post) {
        _post = post;
        _assetsPath = assetsPath;
        _importPath = importPath;
    }

    /// <summary>
    /// Markdown内容解析，复制图片 & 替换图片链接
    /// </summary>
    public string MarkdownParse() {
        if (_post.Content == null) {
            return string.Empty;
        }

        var document = Markdown.Parse(_post.Content);

        foreach (var node in document.AsEnumerable()) {
            if (node is not ParagraphBlock { Inline: { } } paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline) {
                if (inline is not LinkInline { IsImage: true } linkInline) continue;
                if (string.IsNullOrWhiteSpace(linkInline.Url)) continue;

                var imgUrl = Uri.UnescapeDataString(linkInline.Url);
                if (imgUrl.StartsWith("http")) continue;

                var imgPath = Path.Combine(_importPath, _post.Path ?? "", imgUrl);
                var imgFilename = Path.GetFileName(imgUrl);
                var destDir = Path.Combine(_assetsPath, _post.Id);
                if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                var destPath = Path.Combine(destDir, imgFilename);
                if (File.Exists(destPath)) {
                    var imgId = GuidUtils.GuidTo16String();
                    imgFilename =
                        $"{Path.GetFileNameWithoutExtension(imgFilename)}-{imgId}.{Path.GetExtension(imgFilename)}";
                    destPath = Path.Combine(destDir, imgFilename);
                }

                linkInline.Url = imgFilename;
                File.Copy(imgPath, destPath);

                Console.WriteLine($"复制 {imgPath} 到 {destPath}");
            }
        }

        using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }

    /// <summary>
    /// 从文章正文提取前 <paramref name="length"/> 字的梗概
    /// </summary>
    public string GetSummary(int length) {
        return _post.Content == null
            ? string.Empty
            : Markdown.ToPlainText(_post.Content).Limit(length);
    }

    /// <summary>
    /// 填充文章状态和标题
    /// </summary>
    public (string, string) InflateStatusTitle() {
        const string pattern = @"^（(.+)）(.+)$";
        var status = _post.Status ?? "已发布";
        var title = _post.Title;
        if (string.IsNullOrEmpty(title)) return (status, $"未命名文章{_post.CreationTime.ToLongDateString()}");

        var result = Regex.Match(title, pattern);
        if (!result.Success) return (status, title);

        status = result.Groups[1].Value;
        title = result.Groups[2].Value;

        _post.Status = status;
        _post.Title = title;

        if (!new[] { "已发表", "已发布" }.Contains(_post.Status)) {
            _post.IsPublish = false;
        }

        return (status, title);
    }
}
