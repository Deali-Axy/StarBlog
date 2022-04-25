// See https://aka.ms/new-console-template for more information

using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

var filepath = @"D:\blog\Django\项目完成小结-Django3.x版本-开发部署小结.md";
var md = File.ReadAllText(filepath);

var document = Markdown.Parse(md);

foreach (var node in document.AsEnumerable()) {
    if (node is ParagraphBlock { Inline: { } } paragraphBlock) {
        foreach (var inline in paragraphBlock.Inline) {
            if (inline is LinkInline { IsImage: true } linkInline) {
                linkInline.Url = $"http://127.0.0.1:5038/assets/blog/{linkInline.Url}";
                Console.WriteLine(linkInline.Url);
            }
        }
    }
}


// using (var writer = new StringWriter()) {
//     var render = new NormalizeRenderer(writer);
//     render.Render(document);
//
//     Console.WriteLine(writer.ToString());
// }