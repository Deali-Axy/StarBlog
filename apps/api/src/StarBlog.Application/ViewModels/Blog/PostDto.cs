using StarBlog.Data.Models;

namespace StarBlog.Application.ViewModels.Blog;

public sealed class PostDto {
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? Status { get; init; }
    public bool IsPublish { get; init; }
    public string? Summary { get; init; }
    public string? Content { get; init; }
    public string? Path { get; init; }
    public DateTime CreationTime { get; init; }
    public DateTime LastUpdateTime { get; init; }
    public int CategoryId { get; init; }
    public CategoryDto? Category { get; init; }
    public string? Categories { get; init; }

    public static PostDto From(Post post) {
        return new PostDto {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Status = post.Status,
            IsPublish = post.IsPublish,
            Summary = post.Summary,
            Content = post.Content,
            Path = post.Path,
            CreationTime = post.CreationTime,
            LastUpdateTime = post.LastUpdateTime,
            CategoryId = post.CategoryId,
            Category = post.Category == null ? null : CategoryDto.From(post.Category),
            Categories = post.Categories
        };
    }
}
