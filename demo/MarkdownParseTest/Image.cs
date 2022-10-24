namespace MarkdownParseTest;

using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

public class Image {
    async Task<string> DownloadImage(string url) {
        using var httpClient = new HttpClient();
        var resp = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        var fileName = Guid.NewGuid().ToString("N") +
                       Path.GetExtension(url);
        var filePath = Path.Combine("data", "images", fileName);
        using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write)) {
            await resp.Content.CopyToAsync(fs);
        }

        return fileName;
    }

    async void ExtractImagesAndDownload() {
        var filepath = "data/test.md";
        var md = File.ReadAllText(filepath);

        var document = Markdown.Parse(md);

        foreach (var node in document.AsEnumerable()) {
            if (node is ParagraphBlock {Inline: { }} paragraphBlock) {
                foreach (var inline in paragraphBlock.Inline) {
                    if (inline is LinkInline {IsImage: true} linkInline) {
                        if (linkInline.Url == null) continue;

                        Console.WriteLine("download {0}", linkInline.Url);
                        var fileName = await DownloadImage(linkInline.Url);
                        linkInline.Url = Path.Combine("images", fileName);
                        Console.WriteLine("save to {0}", linkInline.Url);
                    }
                }
            }
        }

        using (var writer = new StringWriter()) {
            var render = new NormalizeRenderer(writer);
            render.Render(document);

            using (var sw = new StreamWriter("data/output.md")) {
                await sw.WriteAsync(writer.ToString());
            }

            Console.WriteLine("write to file.");
        }
    }
}