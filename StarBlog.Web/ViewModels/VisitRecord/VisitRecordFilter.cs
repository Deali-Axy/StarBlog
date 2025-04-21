namespace StarBlog.Web.ViewModels.VisitRecord;

public class VisitRecordFilter {
    /// <summary>
    /// 排除 API 接口
    /// </summary>
    public bool? ExcludeApi { get; set; } = false;

    /// <summary>
    /// 排除内网IP
    /// </summary>
    public bool? ExcludeIntranetIp { get; set; }

    public DateTime? Date { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}