namespace StarBlog.Application.ViewModels.VisitRecord;

public class DailyTrend {
    public DateTime Time { get; set; }
    public string Date { get; set; }
    public int Total { get; set; }
    public int Pv { get; set; }
    public int Uv { get; set; }
    public long Api { get; set; }
    public long Spider { get; set; }
}