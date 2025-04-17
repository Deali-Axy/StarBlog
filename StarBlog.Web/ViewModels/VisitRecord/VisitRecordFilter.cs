namespace StarBlog.Web.ViewModels.VisitRecord;

public class VisitRecordFilter {
    public bool ExcludeApi { get; set; } = false;
    public DateTime? Date { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}