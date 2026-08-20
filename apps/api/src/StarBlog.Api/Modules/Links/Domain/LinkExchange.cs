namespace StarBlog.Api.Modules.Links.Domain;

/// <summary>
/// 友情链接申请记录。审核通过后会创建或恢复对应的 Link。
/// </summary>
public sealed class LinkExchange {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public string WebMaster { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Verified { get; set; }
    public string? Reason { get; set; }
    public DateTime ApplyTime { get; set; }
}
