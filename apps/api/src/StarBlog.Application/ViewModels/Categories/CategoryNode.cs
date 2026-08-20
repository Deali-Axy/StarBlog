namespace StarBlog.Application.ViewModels.Categories;

public class CategoryNode {
    public int Id { get; set; }
    public string? text { get; set; }
    public string? href { get; set; }
    public List<string> tags { get; set; }
    public List<CategoryNode>? nodes { get; set; }
}