namespace StarBlog.Application.ViewModels.VisitRecord;

public class GrowthRate {
    public int CurrentPeriodCount { get; set; }
    public int PrevPeriodCount { get; set; }
    public double GrowthPercentage { get; set; }
    public int Days { get; set; }
}