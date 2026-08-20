namespace StarBlog.Api.Modules.Content.Domain;

/// <summary>
/// 文章分类。根分类的 ParentId 为 0。
/// </summary>
public sealed class Category {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ParentId { get; set; }
    public bool Visible { get; set; } = true;
    public List<Post> Posts { get; set; } = [];
}
