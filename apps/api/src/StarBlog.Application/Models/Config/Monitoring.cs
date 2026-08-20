namespace StarBlog.Application.Models.Config;

/// <summary>
/// 监控配置
/// </summary>
public class Monitoring
{
    /// <summary>
    /// Umami 网站分析配置
    /// </summary>
    public UmamiConfig Umami { get; set; } = new();
    
    /// <summary>
    /// Microsoft Clarity 配置
    /// </summary>
    public ClarityConfig Clarity { get; set; } = new();
}

/// <summary>
/// Umami 配置
/// </summary>
public class UmamiConfig
{
    /// <summary>
    /// 是否启用 Umami 分析
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// Umami 脚本地址
    /// </summary>
    public string ScriptUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// 网站 ID
    /// </summary>
    public string WebsiteId { get; set; } = string.Empty;
}

/// <summary>
/// Microsoft Clarity 配置
/// </summary>
public class ClarityConfig
{
    /// <summary>
    /// 是否启用 Clarity 分析
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// Clarity 项目 ID
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;
}