using Markdig;

namespace StarBlog.Api.Modules.Content.Markdown;

/// <summary>
/// 将 Markdown 渲染为带 Bootstrap 样式的 HTML。
/// </summary>
public static class MarkdownRenderer {
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseBootstrap5()
        .Build();

    /// <summary>渲染正文；空内容返回空字符串。</summary>
    public static string ToHtml(string? markdown) => Markdig.Markdown.ToHtml(markdown ?? string.Empty, Pipeline);

    /// <summary>提取纯文本摘要。</summary>
    public static string ToPlainText(string? markdown, int length) {
        var text = Markdig.Markdown.ToPlainText(markdown ?? string.Empty).Trim();
        return text.Length <= length ? text : text[..length];
    }
}
