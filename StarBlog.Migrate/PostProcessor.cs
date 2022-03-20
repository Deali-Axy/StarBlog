using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using StarBlog.Contrib.Extensions;
using StarBlog.Contrib.Utils;
using StarBlog.Data.Models;

namespace StarBlog.Migrate;

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
    /// <returns></returns>
    public string MarkdownParse() {
        var document = Markdown.Parse(_post.Content);

        foreach (var node in document.AsEnumerable()) {
            if (node is not ParagraphBlock {Inline: { }} paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline) {
                if (inline is not LinkInline {IsImage: true} linkInline) continue;

                if (linkInline.Url == null) continue;
                if (linkInline.Url.StartsWith("http")) continue;

                // 路径处理
                var imgPath = Path.Combine(_importPath, _post.Path, linkInline.Url);
                var imgFilename = Path.GetFileName(linkInline.Url);
                var destDir = Path.Combine(_assetsPath, _post.Id);
                if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                var destPath = Path.Combine(destDir, imgFilename);
                if (File.Exists(destPath)) {
                    // 图片重名处理
                    var imgId = GuidUtils.GuidTo16String();
                    imgFilename = $"{Path.GetFileNameWithoutExtension(imgFilename)}-{imgId}.{Path.GetExtension(imgFilename)}";
                    destPath = Path.Combine(destDir, imgFilename);
                }

                // 替换图片链接
                linkInline.Url = imgFilename;
                // 复制图片
                File.Copy(imgPath, destPath);

                Console.WriteLine($"复制 {imgPath} 到 {destPath}");
            }
        }


        using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }

    public string GetSummary(int length) {
        return Markdown.ToPlainText(_post.Content).Limit(length);
    }
}