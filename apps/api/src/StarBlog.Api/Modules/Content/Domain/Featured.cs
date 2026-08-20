namespace StarBlog.Api.Modules.Content.Domain;

/// <summary>推荐文章。</summary>
public sealed class FeaturedPost {
    public int Id { get; set; }
    public string PostId { get; set; } = string.Empty;
    public Post Post { get; set; } = null!;
}

/// <summary>置顶文章。首页只展示一条。</summary>
public sealed class TopPost {
    public int Id { get; set; }
    public string PostId { get; set; } = string.Empty;
    public Post Post { get; set; } = null!;
}

/// <summary>推荐分类，可覆盖展示名称与图标。</summary>
public sealed class FeaturedCategory {
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconCssClass { get; set; } = string.Empty;
}
