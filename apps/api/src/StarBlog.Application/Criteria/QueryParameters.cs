namespace StarBlog.Application.Criteria;

/// <summary>
/// 请求参数
/// </summary>
public class QueryParameters {
    /// <summary>
    /// 最大页面条目
    /// </summary>
    public const int MaxPageSize = 50;

    private int _pageSize = 10;

    /// <summary>
    /// 是否启用分页（默认 true）
    /// </summary>
    public bool EnablePaging { get; set; } = true;

    /// <summary>
    /// 页面大小
    /// </summary>
    public int PageSize {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    public string? SortBy { get; set; }
}