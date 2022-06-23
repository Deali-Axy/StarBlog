namespace StarBlog.Web.ViewModels.QueryFilters;

public class VisitRecordQueryParameters : QueryParameters {
    /// <summary>
    /// 排序字段
    /// </summary>
    public new string? SortBy { get; set; } = "-Time";
}