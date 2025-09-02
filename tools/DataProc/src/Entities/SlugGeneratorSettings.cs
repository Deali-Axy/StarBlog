namespace DataProc.Entities;

public class SlugGeneratorSettings {
    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// 请求间隔时间（毫秒）
    /// </summary>
    public int DelayBetweenRequests { get; set; } = 2000;
    
    /// <summary>
    /// 最大 Slug 长度
    /// </summary>
    public int MaxSlugLength { get; set; } = 50;
    
    /// <summary>
    /// 超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
