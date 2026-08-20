namespace StarBlog.Api.Shared;

/// <summary>
/// 分页查询参数的边界处理。
/// </summary>
public static class Paging {
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    /// <summary>将页码规范到 1 起步。</summary>
    public static int NormalizePage(int page) => Math.Max(page, 1);

    /// <summary>将页大小限制在 1 到 MaxPageSize 之间。</summary>
    public static int NormalizePageSize(int pageSize) => Math.Clamp(pageSize, 1, MaxPageSize);
}
