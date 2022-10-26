using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using StarBlog.Data.Models;

namespace StarBlog.Share.MarkdownExtensions;

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
        for (var i = 0; i < headings.Count; i++) {
            var item = headings[i];
            item.Id = i;

            var text = item.Text ?? "";
            if (Regex.IsMatch(text, "[\u4e00-\u9fbb]")) {
                item.Slug = chineseTitleCount == 0 ? "section" : $"section-{chineseTitleCount}";
                chineseTitleCount++;
            }
            else {
                item.Slug = text.Replace(" ", "-").ToLower();
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
        var doc = Markdown.Parse(post.Content);
        return doc.ExtractToc();
    }
}