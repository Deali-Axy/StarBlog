using System.ComponentModel.DataAnnotations;
using StarBlog.Api.Modules.Links.Domain;

namespace StarBlog.Api.Modules.Links;

/// <summary>公开或管理端返回的友链。</summary>
public sealed class LinkResponse {
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Url { get; init; }
    public bool Visible { get; init; }

    public static LinkResponse From(Link link) => new() {
        Id = link.Id,
        Name = link.Name,
        Description = link.Description,
        Url = link.Url,
        Visible = link.Visible
    };
}

/// <summary>管理端创建或更新友链。</summary>
public sealed class LinkUpsertRequest {
    [Required, StringLength(128)]
    public string Name { get; init; } = string.Empty;

    [StringLength(512)]
    public string? Description { get; init; }

    [Required, Url]
    public string Url { get; init; } = string.Empty;

    public bool Visible { get; init; } = true;
}

/// <summary>前台提交的友链申请。</summary>
public sealed class LinkExchangeApplicationRequest {
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

/// <summary>友链申请对外契约。</summary>
public sealed class LinkExchangeResponse {
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Url { get; init; }
    public required string WebMaster { get; init; }
    public required string Email { get; init; }
    public bool Verified { get; init; }
    public string? Reason { get; init; }
    public DateTime ApplyTime { get; init; }

    public static LinkExchangeResponse From(LinkExchange item) => new() {
        Id = item.Id,
        Name = item.Name,
        Description = item.Description,
        Url = item.Url,
        WebMaster = item.WebMaster,
        Email = item.Email,
        Verified = item.Verified,
        Reason = item.Reason,
        ApplyTime = item.ApplyTime
    };
}

/// <summary>审核通过或拒绝时的补充说明。</summary>
public sealed class LinkExchangeDecisionRequest {
    public string? Reason { get; init; }
}

/// <summary>Site 模块读取公开友链的契约。</summary>
public interface IPublicLinkCatalog {
    Task<IReadOnlyList<LinkResponse>> GetVisibleAsync(CancellationToken cancellationToken = default);
}
