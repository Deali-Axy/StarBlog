using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// 文章评论
/// </summary>
public class Comment : ModelBase {
    [Column(IsIdentity = false, IsPrimary = true)]
    public string Id { get; set; }

    public string? ParentId { get; set; }
    public Comment? Parent { get; set; }
    public List<Comment>? Comments { get; set; }

    public string PostId { get; set; }
    public Post Post { get; set; }

    public string? UserId { get; set; }
    public User? User { get; set; }

    public string? AnonymousUserId { get; set; }
    public AnonymousUser? AnonymousUser { get; set; }

    public string? UserAgent { get; set; }
    public string Content { get; set; }
    public bool Visible { get; set; }

    /// <summary>
    /// 是否需要审核
    /// </summary>
    public bool IsNeedAudit { get; set; } = false;

    /// <summary>
    /// 原因
    /// <para>如果验证不通过的话，可能会附上原因</para>
    /// </summary>
    public string? Reason { get; set; }
}