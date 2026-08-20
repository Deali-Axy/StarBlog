namespace StarBlog.Api.Modules.Content.Domain;

/// <summary>
/// 博客文章。
/// </summary>
public sealed class Post {
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Status { get; set; }
    public bool IsPublish { get; set; }
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? Path { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastUpdateTime { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public string? Categories { get; set; }
    public List<PostTranslation> Translations { get; set; } = [];
}
