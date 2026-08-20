using System.ComponentModel.DataAnnotations;
using StarBlog.Api.Modules.Comments.Domain;

namespace StarBlog.Api.Modules.Comments;

/// <summary>评论对外契约。</summary>
public sealed class CommentResponse {
    public required string Id { get; init; }
    public string? ParentId { get; init; }
    public required string PostId { get; init; }
    public required string Content { get; init; }
    public bool Visible { get; init; }
    public bool IsNeedAudit { get; init; }
    public string? Reason { get; init; }
    public DateTime CreatedTime { get; init; }
    public string? AnonymousUserName { get; init; }
    public List<CommentResponse>? Comments { get; init; }

    public static CommentResponse From(Comment comment, bool includeReplies = false) => new() {
        Id = comment.Id,
        ParentId = comment.ParentId,
        PostId = comment.PostId,
        Content = comment.Content,
        Visible = comment.Visible,
        IsNeedAudit = comment.IsNeedAudit,
        Reason = comment.Reason,
        CreatedTime = comment.CreatedTime,
        AnonymousUserName = comment.AnonymousUser?.Name,
        Comments = includeReplies ? comment.Replies.Select(reply => From(reply, true)).ToList() : null
    };
}

/// <summary>发表评论。</summary>
public sealed class CommentCreateRequest {
    [Required]
    public string PostId { get; init; } = string.Empty;
    public string? ParentId { get; init; }
    [Required]
    public string Content { get; init; } = string.Empty;
    [Required]
    public string UserName { get; init; } = string.Empty;
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
    public string? Url { get; init; }
    [Required]
    public string EmailOtp { get; init; } = string.Empty;
}

/// <summary>审核意见。</summary>
public sealed class CommentDecisionRequest {
    public string? Reason { get; init; }
}
