using StarBlog.Api.Modules.Content.Markdown;

namespace StarBlog.Api.Modules.Content;

/// <summary>
/// 其他模块读取已发布内容的只读契约。
/// </summary>
public interface IPublishedContentQueries {
    Task<PublishedPostSummary?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PublishedPostSummary>> GetPublishedAsync(CancellationToken cancellationToken = default);
    Task<PublishedPostSummary?> GetTopAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PublishedPostSummary>> GetFeaturedPostsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeaturedCategoryResponse>> GetFeaturedCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryResponse>> GetVisibleCategoriesAsync(CancellationToken cancellationToken = default);
}

/// <summary>跨模块使用的已发布文章摘要。</summary>
public sealed class PublishedPostSummary {
    public required string Id { get; init; }
    public required string Title { get; init; }
    public string? Slug { get; init; }
    public string? Summary { get; init; }
    public string? Content { get; init; }
    public DateTime CreationTime { get; init; }
    public DateTime LastUpdateTime { get; init; }
    public int CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public bool IsPublish { get; init; }
}
