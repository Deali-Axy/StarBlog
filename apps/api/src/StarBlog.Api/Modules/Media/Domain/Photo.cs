namespace StarBlog.Api.Modules.Media.Domain;

/// <summary>
/// 摄影作品。
/// </summary>
public sealed class Photo {
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long Height { get; set; }
    public long Width { get; set; }
    public DateTime CreateTime { get; set; }
}

/// <summary>
/// 推荐图片。
/// </summary>
public sealed class FeaturedPhoto {
    public int Id { get; set; }
    public string PhotoId { get; set; } = string.Empty;
    public Photo Photo { get; set; } = null!;
}
