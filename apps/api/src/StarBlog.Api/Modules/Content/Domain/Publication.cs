namespace StarBlog.Api.Modules.Content.Domain;

/// <summary>第三方发布平台。</summary>
public enum PublicationPlatform {
    WechatOfficialAccount = 1,
    Zhihu = 2,
    Juejin = 3,
    Custom = 99
}

/// <summary>一次发布任务的状态。</summary>
public enum PublicationStatus {
    Prepared = 1,
    Publishing = 2,
    Published = 3,
    Failed = 4
}

/// <summary>
/// 管理员配置的发布渠道。AppSecret 永不回传到客户端。
/// </summary>
public sealed class PublicationChannel {
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PublicationPlatform Platform { get; set; }
    public bool Enabled { get; set; } = true;
    public string? AppId { get; set; }
    public string? AppSecret { get; set; }
    public string? Author { get; set; }
    public string? Theme { get; set; }
    public string? PublishUrl { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastUpdateTime { get; set; }
}

/// <summary>
/// 文章投递到渠道后的快照记录。
/// </summary>
public sealed class PostPublication {
    public string Id { get; set; } = string.Empty;
    public string PostId { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public PublicationPlatform Platform { get; set; }
    public PublicationStatus Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string RenderedContent { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string? ExternalId { get; set; }
    public string? ExternalUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastUpdateTime { get; set; }
    public DateTime? PublishedTime { get; set; }
}
