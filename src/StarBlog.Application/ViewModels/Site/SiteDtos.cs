using System.ComponentModel.DataAnnotations;

namespace StarBlog.Application.ViewModels.Site;

/// <summary>
/// 首次部署时创建站点管理员和基础配置的请求。
/// </summary>
public sealed class InitializeSiteDto {
    [Required, StringLength(64)]
    public string Username { get; init; } = string.Empty;

    [Required, StringLength(256, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;

    [Required, Url]
    public string Host { get; init; } = string.Empty;

    [Required]
    public string DefaultRender { get; init; } = "frontend";
}

/// <summary>
/// 面向前台提交的友情链接申请。它与后台审核 DTO 分离，避免客户端提交审核字段。
/// </summary>
public sealed class LinkExchangeApplicationDto {
    [Required, StringLength(128)]
    public string Name { get; init; } = string.Empty;

    [StringLength(512)]
    public string? Description { get; init; }

    [Required, Url]
    public string Url { get; init; } = string.Empty;

    [Required, StringLength(128)]
    public string WebMaster { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
}

/// <summary>
/// 站内搜索的安全输出模型；只返回展示所需的字段，防止把文章内部字段直接泄漏给客户端。
/// </summary>
public sealed class SearchResultItemDto {
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string HighlightedTitle { get; init; } = string.Empty;
    public string HighlightedSnippet { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public DateTime LastUpdateTime { get; init; }
}
