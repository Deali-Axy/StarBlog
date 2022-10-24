using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MarkdownParseTest;

class Heading {
    public int Id { get; set; }
    public int Pid { get; set; } = -1;
    public string? Text { get; set; }
    public int Level { get; set; }
}

public class TocNode {
    public string? Text { get; set; }
    public string? Href { get; set; }
    public List<string>? Tags { get; set; }
    public List<TocNode>? Nodes { get; set; }
}

public static class ToC {
    public static List<TocNode>? ExtractToc(string filepath) {
        var md = File.ReadAllText(filepath);
        var document = Markdown.Parse(md);
        var headings = new List<Heading>();

        foreach (var block in document.AsEnumerable()) {
            if (block is not HeadingBlock heading) continue;
            var item = new Heading {Level = heading.Level, Text = heading.Inline?.FirstChild?.ToString()};
            headings.Add(item);
            Console.WriteLine($"{new string('#', item.Level)} {item.Text}");
        }

        for (var i = 0; i < headings.Count; i++) {
            var item = headings[i];
            item.Id = i;
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
                : nodes.Select(a => new TocNode {Text = a.Text, Href = $"#{a.Text}", Nodes = GetNodes(a.Id)}).ToList();
        }

        return GetNodes();
    }
}