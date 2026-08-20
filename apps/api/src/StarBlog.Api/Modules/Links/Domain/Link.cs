namespace StarBlog.Api.Modules.Links.Domain;

/// <summary>
/// 友情链接。
/// </summary>
public sealed class Link {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool Visible { get; set; }
}
