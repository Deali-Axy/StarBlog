namespace StarBlog.Web.ViewModels;

public static class LinkTarget {
    public const string Blank = "_blank";
    public const string Parent = "_parent";
    public const string Self = "_self";
    public const string Top = "_top";
}

public class ColorfulButtonViewModel {
    public string Name { get; set; }
    public string Url { get; set; } = "#";
    public string Target { get; set; } = LinkTarget.Blank;
    public string? Tooltips { get; set; }
}