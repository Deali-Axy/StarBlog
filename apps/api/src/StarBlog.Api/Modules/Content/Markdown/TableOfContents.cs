using System.Text;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace StarBlog.Api.Modules.Content.Markdown;

internal sealed class Heading {
    public int Id { get; set; }
    public int Pid { get; set; } = -1;
    public string? Text { get; set; }
    public string? Slug { get; set; }
    public int Level { get; set; }
}

/// <summary>目录节点。</summary>
public sealed class TocNode {
    public string? Text { get; set; }
    public string? Href { get; set; }
    public List<string>? Tags { get; set; }
    public List<TocNode>? Nodes { get; set; }
}

/// <summary>
/// 从 Markdown 正文提取标题树。
/// </summary>
public static class TableOfContents {
    /// <summary>内容为空时返回 null。</summary>
    public static List<TocNode>? Extract(string? content) {
        if (content == null) return null;

        var pipeline = new MarkdownPipelineBuilder().UseAutoIdentifiers().Build();
        var document = Markdig.Markdown.Parse(content, pipeline);
        _ = document.ToHtml(pipeline);

        var headings = document.Descendants<HeadingBlock>()
            .Select((heading, index) => new Heading {
                Id = index,
                Text = GetHeadingText(heading),
                Slug = heading.GetAttributes().Id,
                Level = heading.Level
            })
            .ToList();

        for (var i = 0; i < headings.Count; i++) {
            var current = headings[i];
            for (var j = i - 1; j >= 0; j--) {
                if (headings[j].Level >= current.Level) continue;
                current.Pid = headings[j].Id;
                break;
            }
        }

        var roots = new List<TocNode>();
        var map = new Dictionary<int, TocNode>();
        foreach (var heading in headings) {
            var node = new TocNode { Text = heading.Text, Href = $"#{heading.Slug}" };
            map[heading.Id] = node;
            if (heading.Pid == -1) {
                roots.Add(node);
            }
            else {
                var parent = map[heading.Pid];
                parent.Nodes ??= [];
                parent.Nodes.Add(node);
            }
        }

        return roots;
    }

    private static string GetHeadingText(HeadingBlock heading) {
        if (heading.Inline == null) return string.Empty;
        var builder = new StringBuilder();
        foreach (var inline in heading.Inline.Descendants<LiteralInline>()) {
            builder.Append(inline.Content);
        }

        return builder.ToString();
    }
}
