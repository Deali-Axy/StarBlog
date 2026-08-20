namespace StarBlog.Application.ViewModels.VisitRecord;

public class ConversionStats {
    public string TargetPath { get; set; }
    public int TotalSessions { get; set; }
    public int ConvertedSessions { get; set; }
    public double ConversionRate { get; set; }
    public List<NameCountPair> TopEntryPages { get; set; }
}