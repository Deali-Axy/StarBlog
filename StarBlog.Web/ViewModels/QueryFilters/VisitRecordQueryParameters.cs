namespace StarBlog.Web.ViewModels.QueryFilters;

public class VisitRecordQueryParameters : QueryParameters {
    /// <summary>
    /// 排序字段
    /// </summary>
    public new string? SortBy { get; set; } = "-Time";
    
    public string? Country { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? Isp { get; set; }
}