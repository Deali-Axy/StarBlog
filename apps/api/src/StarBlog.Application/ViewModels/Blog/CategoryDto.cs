using StarBlog.Data.Models;

namespace StarBlog.Application.ViewModels.Blog;

public sealed class CategoryDto {
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int ParentId { get; init; }

    public static CategoryDto From(Category category) {
        return new CategoryDto {
            Id = category.Id,
            Name = category.Name,
            ParentId = category.ParentId
        };
    }
}
