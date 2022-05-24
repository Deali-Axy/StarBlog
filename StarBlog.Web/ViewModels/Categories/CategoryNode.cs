namespace StarBlog.Web.ViewModels.Categories;

public class CategoryNode {
    public string text { get; set; } = "";
    public string href { get; set; } = "";
    public List<CategoryNode>? nodes { get; set; }
}