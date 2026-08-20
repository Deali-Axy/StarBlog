using Markdig;
using Markdig.Extensions.Figures;
using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace StarBlog.Api.Modules.Content.Markdown;

/// <summary>
/// 为部分 HTML 元素附加 Bootstrap 5 样式类。
/// </summary>
public sealed class Bootstrap5Extension : IMarkdownExtension {
    public void Setup(MarkdownPipelineBuilder pipeline) {
        pipeline.DocumentProcessed -= PipelineOnDocumentProcessed;
        pipeline.DocumentProcessed += PipelineOnDocumentProcessed;
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) {
    }

    private static void PipelineOnDocumentProcessed(MarkdownDocument document) {
        foreach (var node in document.Descendants()) {
            if (node is Inline) {
                if (node is LinkInline { IsImage: true } link) {
                    link.GetAttributes().AddClass("img-fluid");
                }
            }
            else if (node is ContainerBlock) {
                switch (node) {
                    case Table:
                        node.GetAttributes().AddClass("table");
                        break;
                    case QuoteBlock:
                        node.GetAttributes().AddClass("blockquote");
                        break;
                    case Figure:
                        node.GetAttributes().AddClass("figure");
                        break;
                    case ListItemBlock:
                        node.GetAttributes().AddClass("my-1");
                        break;
                }
            }
            else if (node is FigureCaption) {
                node.GetAttributes().AddClass("figure-caption");
            }
        }
    }
}

/// <summary>MarkdownPipeline 扩展。</summary>
public static class MarkdownPipelineExtensions {
    public static MarkdownPipelineBuilder UseBootstrap5(this MarkdownPipelineBuilder pipeline) {
        pipeline.Extensions.AddIfNotAlready<Bootstrap5Extension>();
        return pipeline;
    }
}
