using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>第三方发布平台的类型。</summary>
public enum PublicationPlatform {
    WechatOfficialAccount = 1,
    Zhihu = 2,
    Juejin = 3,
    Custom = 99
}

/// <summary>一次发布任务的生命周期状态。</summary>
public enum PublicationStatus {
    Prepared = 1,
    Publishing = 2,
    Published = 3,
    Failed = 4
}

/// <summary>
/// 管理员配置的一个发布渠道。
///
/// 账号密钥只会在服务端使用，API 输出 DTO 永不回传 <see cref="AppSecret"/>。
/// </summary>
public class PublicationChannel {
    [Column(IsPrimary = true, IsIdentity = false)]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public PublicationPlatform Platform { get; set; }
    public bool Enabled { get; set; } = true;

    /// <summary>公众号 AppId，或其他平台所需的公开标识。</summary>
    public string? AppId { get; set; }

    /// <summary>
    /// 平台私密凭证。生产环境应由数据保护/密钥管理服务加密数据库；该字段不会被客户端读取。
    /// </summary>
    public string? AppSecret { get; set; }

    public string? Author { get; set; }
    public string? Theme { get; set; }
    public string? PublishUrl { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastUpdateTime { get; set; }
}
