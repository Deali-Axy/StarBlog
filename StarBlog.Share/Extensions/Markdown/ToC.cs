using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using StarBlog.Data.Models;

namespace StarBlog.Share.Extensions.Markdown;

class Heading {
    public int Id { get; set; }
    public int Pid { get; set; } = -1;
    public string? Text { get; set; }
    public string? Slug { get; set; }
    public int Level { get; set; }
}

public class TocNode {
    public string? Text { get; set; }
    public string? Href { get; set; }
    public List<string>? Tags { get; set; }
    public List<TocNode>? Nodes { get; set; }
}

public static class ToC {
    public static List<TocNode>? ExtractToc(this MarkdownDocument document) {
        var headings = new List<Heading>();

        foreach (var heading in document.Descendants<HeadingBlock>()) {
            Heading item;

            // 处理标题中包含特殊语法（如代码块、加粗等）
            if (heading.Inline != null) {
                var textBuilder = new StringBuilder();
                foreach (var inline in heading.Inline) {
                    switch (inline) {
                        case CodeInline codeInline:
                            textBuilder.Append(codeInline.Content);
                            break;
                        case EmphasisInline emphasisInline:
                            foreach (var emphasisContent in emphasisInline) {
                                textBuilder.Append(emphasisContent);
                            }

                            break;
                        case LiteralInline literalInline:
                            textBuilder.Append(literalInline.Content);
                            break;
                        default:
                            textBuilder.Append(inline);
                            break;
                    }
                }

                // 去除空格和换行符
                string cleanedText = Regex.Replace(textBuilder.ToString().Trim(), @"[\t\n\r]", "");
                item = new Heading { Level = heading.Level, Text = cleanedText };
            }
            else {
                item = new Heading { Level = heading.Level, Text = null };
            }


            headings.Add(item);
        }

        var chineseTitleCount = 0;
        var slugMap = new Dictionary<string, int>();
        for (var i = 0; i < headings.Count; i++) {
            var item = headings[i];
            item.Id = i;

            var text = item.Text ?? "";
            // 包含中文且不包含英文的转换为 section-1 格式
            if (Regex.IsMatch(text, "^((?![a-zA-Z]).)*[\u4e00-\u9fbb]((?![a-zA-Z]).)*$")) {
                item.Slug = chineseTitleCount == 0 ? "section" : $"section-{chineseTitleCount}";
                chineseTitleCount++;
            }
            // 其他情况处理为只包含英文数字格式
            else {
                item.Slug = Regex.Replace(text, @"[^a-zA-Z0-9\s]+", "")
                    .Trim().Replace(" ", "-").ToLower();
                if (slugMap.ContainsKey(item.Slug)) {
                    item.Slug = $"{item.Slug}-{slugMap[item.Slug]++}";
                }
                else {
                    slugMap[item.Slug] = 1;
                }
            }

            for (var j = i; j >= 0; j--) {
                var preItem = headings[j];
                if (item.Level == preItem.Level + 1) {
                    item.Pid = j;
                    break;
                }
            }
        }

        List<TocNode>? GetNodes(int pid = -1) {
            var nodes = headings.Where(a => a.Pid == pid).ToList();
            return nodes.Count == 0
                ? null
                : nodes.Select(a => new TocNode { Text = a.Text, Href = $"#{a.Slug}", Nodes = GetNodes(a.Id) })
                    .ToList();
        }

        return GetNodes();
    }

    private static string GetHeadingText(HeadingBlock heading) {
        if (heading.Inline == null) return string.Empty;

        var stringBuilder = new StringBuilder();
        foreach (var inline in heading.Inline.Descendants<LiteralInline>()) {
            stringBuilder.Append(inline.Content);
        }

        return stringBuilder.ToString();
    }

    public static List<TocNode>? ExtractToc(this Post post) {
        if (post.Content == null) return null;

        var pipeline = new MarkdownPipelineBuilder()
            .UseAutoIdentifiers()
            .Build();

        var document = Markdig.Markdown.Parse(post.Content, pipeline);

        // Markdig 中获取标题 ID 的正确方式是通过 GetAttributes() 方法，但需要在渲染完成后才能获取。
        // 因为 ID 的生成是在 HeadingBlock_ProcessInlinesEnd 阶段完成的 (参考源码: src\Markdig\Extensions\AutoIdentifiers\AutoIdentifierExtension.cs)
        _ = document.ToHtml(pipeline);

        // 1. 先将所有标题转换为扁平结构
        var headings = document.Descendants<HeadingBlock>()
            .Select((heading, index) => new Heading {
                Id = index,
                Text = GetHeadingText(heading),
                Slug = heading.GetAttributes().Id,
                Level = heading.Level
            })
            .ToList();

        // 2. 建立父子关系
        for (var i = 0; i < headings.Count; i++) {
            var current = headings[i];
            // 向前查找第一个级别小于当前标题的标题作为父标题
            for (int j = i - 1; j >= 0; j--) {
                if (headings[j].Level < current.Level) {
                    current.Pid = headings[j].Id;
                    break;
                }
            }
        }

        // 3. 转换为树状结构
        var tocNodes = new List<TocNode>();
        var nodeMap = new Dictionary<int, TocNode>();

        foreach (var heading in headings) {
            var node = new TocNode {
                Text = heading.Text,
                Href = $"#{heading.Slug}"
            };
            nodeMap[heading.Id] = node;

            if (heading.Pid == -1) {
                // 根节点
                tocNodes.Add(node);
            }
            else {
                // 子节点
                var parentNode = nodeMap[heading.Pid];
                if (parentNode.Nodes == null) {
                    parentNode.Nodes = new List<TocNode>();
                }
                parentNode.Nodes.Add(node);
            }
        }

        return tocNodes;
    }
}