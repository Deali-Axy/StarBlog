using System.ComponentModel.DataAnnotations;
using StarBlog.Api.Modules.Content.Domain;
using StarBlog.Api.Modules.Content.Markdown;

namespace StarBlog.Api.Modules.Content;

/// <summary>分类对外契约。</summary>
public sealed class CategoryResponse {
    public int Id { get; init; }
    public required string Name { get; init; }
    public int ParentId { get; init; }
    public bool Visible { get; init; }
}

/// <summary>分类树节点，tags[0] 为文章总数。</summary>
public sealed class CategoryNodeResponse {
    public int Id { get; init; }
    public required string Text { get; init; }
    public required string Href { get; init; }
    public List<string> Tags { get; init; } = [];
    public List<CategoryNodeResponse>? Nodes { get; init; }
}

/// <summary>创建或更新分类。</summary>
public sealed class CategoryUpsertRequest {
    [Required, StringLength(128)]
    public string Name { get; init; } = string.Empty;
    public int ParentId { get; init; }
    public bool Visible { get; init; } = true;
}

/// <summary>推荐分类。</summary>
public sealed class FeaturedCategoryResponse {
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string IconCssClass { get; init; }
}

/// <summary>创建推荐分类。</summary>
public sealed class FeaturedCategoryCreateRequest {
    [Required]
    public string Name { get; init; } = string.Empty;
    [Required]
    public string Description { get; init; } = string.Empty;
    [Required]
    public string IconCssClass { get; init; } = string.Empty;
}

/// <summary>文章对外契约。</summary>
public sealed class PostResponse {
    public required string Id { get; init; }
    public required string Title { get; init; }
    public string? Slug { get; init; }
    public string? Status { get; init; }
    public bool IsPublish { get; init; }
    public string? Summary { get; init; }
    public string? Content { get; init; }
    public string? Path { get; init; }
    public DateTime CreationTime { get; init; }
    public DateTime LastUpdateTime { get; init; }
    public int CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? Categories { get; init; }
    public List<TocNode>? Toc { get; init; }

    public static PostResponse From(Post post, bool includeToc = false) => new() {
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
        CategoryName = post.Category?.Name,
        Categories = post.Categories,
        Toc = includeToc ? TableOfContents.Extract(post.Content) : null
    };
}

/// <summary>创建或更新文章。</summary>
public sealed class PostUpsertRequest {
    [Required, StringLength(256)]
    public string Title { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? Status { get; init; }
    public bool IsPublish { get; init; }
    public string? Summary { get; init; }
    public string? Content { get; init; }
    public int CategoryId { get; init; } = 1;
}

/// <summary>译文契约。</summary>
public sealed class PostTranslationResponse {
    public required string Id { get; init; }
    public required string PostId { get; init; }
    public required string Language { get; init; }
    public required string Title { get; init; }
    public string? Summary { get; init; }
    public string? Content { get; init; }
}

/// <summary>发布渠道对外契约，永不包含 AppSecret。</summary>
public sealed class PublicationChannelResponse {
    public required string Id { get; init; }
    public required string Name { get; init; }
    public PublicationPlatform Platform { get; init; }
    public bool Enabled { get; init; }
    public string? AppId { get; init; }
    public bool HasSecret { get; init; }
    public string? Author { get; init; }
    public string? Theme { get; init; }
    public string? PublishUrl { get; init; }

    public static PublicationChannelResponse From(PublicationChannel channel) => new() {
        Id = channel.Id,
        Name = channel.Name,
        Platform = channel.Platform,
        Enabled = channel.Enabled,
        AppId = channel.AppId,
        HasSecret = !string.IsNullOrWhiteSpace(channel.AppSecret),
        Author = channel.Author,
        Theme = channel.Theme,
        PublishUrl = channel.PublishUrl
    };
}

/// <summary>创建或更新发布渠道。</summary>
public sealed class PublicationChannelUpsertRequest {
    [Required, StringLength(80)]
    public string Name { get; init; } = string.Empty;
    public PublicationPlatform Platform { get; init; }
    public bool Enabled { get; init; } = true;
    public string? AppId { get; init; }
    public string? AppSecret { get; init; }
    public string? Author { get; init; }
    public string? Theme { get; init; }
    public string? PublishUrl { get; init; }
}

/// <summary>准备发布快照。</summary>
public sealed class PreparePublicationRequest {
    [Required]
    public string ChannelId { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? CoverUrl { get; init; }
}

/// <summary>发布记录对外契约。</summary>
public sealed class PostPublicationResponse {
    public required string Id { get; init; }
    public required string PostId { get; init; }
    public required string ChannelId { get; init; }
    public PublicationPlatform Platform { get; init; }
    public PublicationStatus Status { get; init; }
    public required string Title { get; init; }
    public required string RenderedContent { get; init; }
    public string? CoverUrl { get; init; }
    public string? ExternalId { get; init; }
    public string? ExternalUrl { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime CreationTime { get; init; }
    public DateTime LastUpdateTime { get; init; }
    public DateTime? PublishedTime { get; init; }

    public static PostPublicationResponse From(PostPublication publication) => new() {
        Id = publication.Id,
        PostId = publication.PostId,
        ChannelId = publication.ChannelId,
        Platform = publication.Platform,
        Status = publication.Status,
        Title = publication.Title,
        RenderedContent = publication.RenderedContent,
        CoverUrl = publication.CoverUrl,
        ExternalId = publication.ExternalId,
        ExternalUrl = publication.ExternalUrl,
        ErrorMessage = publication.ErrorMessage,
        CreationTime = publication.CreationTime,
        LastUpdateTime = publication.LastUpdateTime,
        PublishedTime = publication.PublishedTime
    };
}

/// <summary>博客概况。</summary>
public sealed class BlogOverviewResponse {
    public long PostsCount { get; init; }
    public long CategoriesCount { get; init; }
    public long PhotosCount { get; init; }
    public long FeaturedPostsCount { get; init; }
    public long FeaturedCategoriesCount { get; init; }
    public long FeaturedPhotosCount { get; init; }
}
