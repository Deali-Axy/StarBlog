namespace StarBlog.Application.ViewModels.Categories;

public record CategoryCreationDto {
    public string Name { get; set; }
    public int ParentId { get; set; }
    public bool Visible { get; set; } = true;
}