using StarBlog.Api.Modules.Content;
using StarBlog.Api.Modules.Links;
using StarBlog.Api.Modules.Media;

namespace StarBlog.Api.Modules.Site;

/// <summary>首页聚合数据。</summary>
public sealed class HomeResponse {
    public bool ChartVisible { get; init; }
    public bool RandomPhotoVisible { get; init; }
    public PhotoResponse? RandomPhoto { get; init; }
    public PublishedPostSummary? TopPost { get; init; }
    public IReadOnlyList<PublishedPostSummary> FeaturedPosts { get; init; } = [];
    public IReadOnlyList<PhotoResponse> FeaturedPhotos { get; init; } = [];
    public IReadOnlyList<FeaturedCategoryResponse> FeaturedCategories { get; init; } = [];
    public IReadOnlyList<LinkResponse> Links { get; init; } = [];
}

/// <summary>搜索命中。</summary>
public sealed class SearchResultResponse {
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string HighlightedTitle { get; init; }
    public required string HighlightedSnippet { get; init; }
    public string? Slug { get; init; }
    public DateTime LastUpdateTime { get; init; }
}

/// <summary>Bootswatch 主题。</summary>
public sealed class ThemeResponse {
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required string CssUrl { get; init; }
}
