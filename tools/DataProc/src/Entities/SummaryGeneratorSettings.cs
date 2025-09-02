namespace DataProc.Entities;

public class SummaryGeneratorSettings {
    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// 请求间隔时间（毫秒）
    /// </summary>
    public int DelayBetweenRequests { get; set; } = 2000;
    
    /// <summary>
    /// 最大内容长度（字符数）
    /// </summary>
    public int MaxContentLength { get; set; } = 8000;
    
    /// <summary>
    /// 批处理大小
    /// </summary>
    public int BatchSize { get; set; } = 10;
    
    /// <summary>
    /// 是否启用并行处理
    /// </summary>
    public bool EnableParallelProcessing { get; set; } = false;
    
    /// <summary>
    /// 最大并发数
    /// </summary>
    public int MaxConcurrency { get; set; } = 2;
    
    /// <summary>
    /// 超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;
}
