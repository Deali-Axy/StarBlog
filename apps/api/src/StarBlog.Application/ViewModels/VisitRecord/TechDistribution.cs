namespace StarBlog.Application.ViewModels.VisitRecord;

public class TechDistribution {
    public List<NameCountPair> Browsers { get; set; }
    public List<NameCountPair> OperatingSystems { get; set; }
    public List<NameCountPair> Devices { get; set; }
}

public class NameCountPair {
    public string Name { get; set; }
    public int Count { get; set; }
}