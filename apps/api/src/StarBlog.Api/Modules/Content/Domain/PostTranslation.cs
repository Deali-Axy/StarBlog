namespace StarBlog.Api.Modules.Content.Domain;

/// <summary>
/// 文章的一种语言译本。
/// </summary>
public sealed class PostTranslation {
    public string Id { get; set; } = string.Empty;
    public string PostId { get; set; } = string.Empty;
    public Post Post { get; set; } = null!;
    public string Language { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastUpdateTime { get; set; }
}
