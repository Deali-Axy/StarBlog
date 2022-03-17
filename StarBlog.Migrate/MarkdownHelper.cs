using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace StarBlog.Migrate;

public static class MarkdownHelper {
    /// <summary>
    /// Markdown内容解析
    /// </summary>
    /// <returns></returns>
    public static string Parse(string postId, string postFilePath, string md) {
        var document = Markdown.Parse(md);

        foreach (var node in document.AsEnumerable()) {
            if (node is not ParagraphBlock {Inline: { }} paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline) {
                if (inline is not LinkInline {IsImage: true} linkInline) continue;
                linkInline.Url = $"http://127.0.0.1:5038/assets/blog/{linkInline.Url}";
                Console.WriteLine(linkInline.Url);
            }
        }


        using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }
}