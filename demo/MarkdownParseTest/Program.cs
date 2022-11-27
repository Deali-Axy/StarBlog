// See https://aka.ms/new-console-template for more information

using System.Text.Encodings.Web;
using System.Text.Json;
using MarkdownParseTest;

var nodes= ToC.ExtractToc("data/test.md");
// Console.Read();
Console.WriteLine(JsonSerializer.Serialize(nodes, new JsonSerializerOptions {
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
}));