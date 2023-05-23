using System.Text.RegularExpressions;
using Markdig.Syntax;
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
            var item = new Heading {Level = heading.Level, Text = heading.Inline?.FirstChild?.ToString()};
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
                : nodes.Select(a => new TocNode {Text = a.Text, Href = $"#{a.Slug}", Nodes = GetNodes(a.Id)}).ToList();
        }

        return GetNodes();
    }

    public static List<TocNode>? ExtractToc(this Post post) {
        if (post.Content == null) return null;
        var doc = Markdig.Markdown.Parse(post.Content);
        return doc.ExtractToc();
    }
}