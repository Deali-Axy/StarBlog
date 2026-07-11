namespace StarBlog.Web.ViewModels.Categories;

public record CategoryCreationDto {
    public string Name { get; set; } = string.Empty;
    public int ParentId { get; set; }
    public bool Visible { get; set; } = true;
}
