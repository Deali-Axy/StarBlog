using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// 一篇 StarBlog 文章投递到一个渠道后的可追踪记录。
/// 保留渲染后的内容快照，确保后续编辑原文不会篡改已经提交的平台版本。
/// </summary>
public class PostPublication {
    [Column(IsPrimary = true, IsIdentity = false)]
    public string Id { get; set; } = string.Empty;
    public string PostId { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public PublicationPlatform Platform { get; set; }
    public PublicationStatus Status { get; set; }
    public string Title { get; set; } = string.Empty;

    [MaxLength(-1)]
    public string RenderedContent { get; set; } = string.Empty;

    public string? CoverUrl { get; set; }
    public string? ExternalId { get; set; }
    public string? ExternalUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastUpdateTime { get; set; }
    public DateTime? PublishedTime { get; set; }
}
