namespace StarBlog.Api.Modules.Comments.Domain;

/// <summary>
/// 文章评论。
/// </summary>
public sealed class Comment {
    public string Id { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public Comment? Parent { get; set; }
    public List<Comment> Replies { get; set; } = [];
    public string PostId { get; set; } = string.Empty;
    public string? AnonymousUserId { get; set; }
    public AnonymousUser? AnonymousUser { get; set; }
    public string? UserAgent { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool Visible { get; set; }
    public bool IsNeedAudit { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedTime { get; set; }
}

/// <summary>
/// 匿名评论用户，按邮箱去重。
/// </summary>
public sealed class AnonymousUser {
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Ip { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedTime { get; set; }
}
