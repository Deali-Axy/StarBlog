using Markdig;
using Markdig.Syntax;

namespace MarkdownParseTest;

class Header {
    public string? Text { get; set; }
    public int Level { get; set; }
}

class TocNode {
    public string? Text { get; set; }
    public string Href { get; set; }
    public List<string> Tags { get; set; }
    public List<TocNode> Nodes { get; set; }
}

public static class ToC {
    public static void ExtractToc(string filepath) {
        var md = File.ReadAllText(filepath);
        var document = Markdown.Parse(md);
        var headers = new List<Header>();

        foreach (var node in document.AsEnumerable()) {
            if (node is HeadingBlock block) {
                Console.WriteLine("{0}: {1}", block.Level, block.Inline?.FirstChild);
                headers.Add(new Header { Level = block.Level, Text = block.Inline?.FirstChild?.ToString() });
            }
        }
    }
}