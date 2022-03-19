namespace StarBlog.Web.ViewModels.QueryFilters; 

/// <summary>
/// 博客文章请求参数
/// </summary>
public class PostQueryParameters : QueryParameters {
    /// <summary>
    /// 分类ID
    /// </summary>
    public int CategoryId { get; set; } = 0;

    /// <summary>
    /// 排序字段
    /// </summary>
    public new string? SortBy { get; set; } = "LastUpdateTime";
}