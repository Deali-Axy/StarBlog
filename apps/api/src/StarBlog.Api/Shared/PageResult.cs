namespace StarBlog.Api.Shared;

/// <summary>
/// 简单分页结果。各模块 HTTP 契约复用此类型，避免引入第三方分页库。
/// </summary>
public sealed class PageResult<T> {
    public required IReadOnlyList<T> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
