// See https://aka.ms/new-console-template for more information

using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

var filepath = @"E:\Documents\0_Write\0_blog\机器学习\多个约束条件下的二维装箱问题——寻找《开罗拉面店》最优布局.md";
var md = File.ReadAllText(filepath);

var document = Markdown.Parse(md);


// Takes note of all of the Top Level Headers.
foreach (var node in document.AsEnumerable()) {
    if (node is ParagraphBlock paragraphBlock) {
        if (paragraphBlock.Inline != null) {
            foreach (var inline in paragraphBlock.Inline) {
                if (inline is LinkInline {IsImage: true} linkInline) {
                    linkInline.Url = $"http://127.0.0.1:5038/assets/blog/{linkInline.Url}";
                    Console.WriteLine(linkInline.Url);
                }
            }
        }
    }
}


using (var writer = new StringWriter()) {
    var render = new NormalizeRenderer(writer);
    render.Render(document);

    Console.WriteLine(writer.ToString());
}

