using StarBlog.Data.Models;

namespace StarBlog.Application.ViewModels.Blog;

public sealed class FeaturedPostDto {
    public int Id { get; init; }
    public string PostId { get; init; } = string.Empty;
    public PostDto? Post { get; init; }

    public static FeaturedPostDto From(FeaturedPost item) {
        return new FeaturedPostDto {
            Id = item.Id,
            PostId = item.PostId,
            Post = item.Post == null ? null : PostDto.From(item.Post)
        };
    }
}
