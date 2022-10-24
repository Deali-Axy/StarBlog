using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MarkdownParseTest;

class Header {
    public string Text { get; set; }
    public int Level { get; set; }
}

public static class ToC {
    public static void ExtractToc(string filepath) {
        var md = File.ReadAllText(filepath);
        var document = Markdown.Parse(md);
        
        foreach (var node in document.AsEnumerable()) {
            if (node is ParagraphBlock {Inline: { }} paragraphBlock) {
                foreach (var inline in paragraphBlock.Inline) {
                    Console.WriteLine(inline);
                }
                Console.WriteLine(paragraphBlock.Inline);   
            }
        }
    }
}