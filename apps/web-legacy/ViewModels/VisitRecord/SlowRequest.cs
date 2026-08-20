namespace StarBlog.Web.ViewModels.VisitRecord;

public class SlowRequest {
    public string Path { get; set; }
    public string Method { get; set; }
    public long ResponseTimeMs { get; set; }
    public DateTime Time { get; set; }
    public int StatusCode { get; set; }
}