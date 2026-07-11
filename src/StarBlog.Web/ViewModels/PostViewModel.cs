using System.Text.Encodings.Web;
using System.Text.Json;
using StarBlog.Data.Models;
using StarBlog.Content.Extensions.Markdown;

namespace StarBlog.Web.ViewModels;

public class PostViewModel {
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ContentHtml { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? Url { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastUpdateTime { get; set; }
    public Category Category { get; set; } = new();
    public List<Category> Categories { get; set; } = [];
    public List<TocNode>? TocNodes { get; set; }

    /// <summary>
    /// 当前语言（null 或 "zh" 表示中文原文）
    /// </summary>
    public string? CurrentLanguage { get; set; }

    /// <summary>
    /// 可用的翻译语言列表
    /// </summary>
    public List<string> AvailableLanguages { get; set; } = new();

    /// <summary>
    /// 当前是否显示翻译版本
    /// </summary>
    public bool IsTranslation => !string.IsNullOrEmpty(CurrentLanguage) && CurrentLanguage != "zh";

    public string TocNodesJson => JsonSerializer.Serialize(
        TocNodes,
        new JsonSerializerOptions {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }
    );
}
