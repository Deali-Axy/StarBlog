using System.ComponentModel.DataAnnotations;
using StarBlog.Data.Models;

namespace StarBlog.Application.ViewModels.Publishing;

/// <summary>发布渠道的创建/更新输入。空的 AppSecret 表示更新时保留已存密钥。</summary>
public sealed class PublicationChannelUpsertDto {
    [Required, StringLength(80)]
    public string Name { get; init; } = string.Empty;
    [Required]
    public PublicationPlatform Platform { get; init; }
    public bool Enabled { get; init; } = true;
    public string? AppId { get; init; }
    public string? AppSecret { get; init; }
    public string? Author { get; init; }
    public string? Theme { get; init; }
    [Url]
    public string? PublishUrl { get; init; }
}

/// <summary>可安全返回给管理端的渠道信息。</summary>
public sealed class PublicationChannelDto {
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public PublicationPlatform Platform { get; init; }
    public bool Enabled { get; init; }
    public string? AppId { get; init; }
    public bool HasSecret { get; init; }
    public string? Author { get; init; }
    public string? Theme { get; init; }
    public string? PublishUrl { get; init; }

    public static PublicationChannelDto From(PublicationChannel channel) => new() {
        Id = channel.Id, Name = channel.Name, Platform = channel.Platform, Enabled = channel.Enabled,
        AppId = channel.AppId, HasSecret = !string.IsNullOrWhiteSpace(channel.AppSecret),
        Author = channel.Author, Theme = channel.Theme, PublishUrl = channel.PublishUrl
    };
}

/// <summary>创建一个可预览、可再次发布的文章投递快照。</summary>
public sealed class PreparePublicationDto {
    [Required]
    public string ChannelId { get; init; } = string.Empty;
    public string? Title { get; init; }
    [Url]
    public string? CoverUrl { get; init; }
}

/// <summary>发布记录的管理端输出。</summary>
public sealed class PostPublicationDto {
    public string Id { get; init; } = string.Empty;
    public string PostId { get; init; } = string.Empty;
    public string ChannelId { get; init; } = string.Empty;
    public PublicationPlatform Platform { get; init; }
    public PublicationStatus Status { get; init; }
    public string Title { get; init; } = string.Empty;
    public string RenderedContent { get; init; } = string.Empty;
    public string? CoverUrl { get; init; }
    public string? ExternalId { get; init; }
    public string? ExternalUrl { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime CreationTime { get; init; }
    public DateTime LastUpdateTime { get; init; }
    public DateTime? PublishedTime { get; init; }

    public static PostPublicationDto From(PostPublication publication) => new() {
        Id = publication.Id, PostId = publication.PostId, ChannelId = publication.ChannelId,
        Platform = publication.Platform, Status = publication.Status, Title = publication.Title,
        RenderedContent = publication.RenderedContent, CoverUrl = publication.CoverUrl,
        ExternalId = publication.ExternalId, ExternalUrl = publication.ExternalUrl,
        ErrorMessage = publication.ErrorMessage, CreationTime = publication.CreationTime,
        LastUpdateTime = publication.LastUpdateTime, PublishedTime = publication.PublishedTime
    };
}
