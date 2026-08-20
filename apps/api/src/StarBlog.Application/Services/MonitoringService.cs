using Microsoft.Extensions.Options;
using StarBlog.Application.Models.Config;

namespace StarBlog.Application.Services;

/// <summary>
/// 监控服务，用于管理网站分析和监控配置
/// </summary>
public class MonitoringService
{
    private readonly Monitoring _config;

    public MonitoringService(IOptions<Monitoring> options)
    {
        _config = options.Value;
    }

    /// <summary>
    /// 获取 Umami 配置
    /// </summary>
    public UmamiConfig UmamiConfig => _config.Umami;

    /// <summary>
    /// 获取 Clarity 配置
    /// </summary>
    public ClarityConfig ClarityConfig => _config.Clarity;

    /// <summary>
    /// 检查是否启用了任何监控服务
    /// </summary>
    public bool IsAnyMonitoringEnabled => _config.Umami.Enabled || _config.Clarity.Enabled;

    /// <summary>
    /// 检查是否启用了 Umami
    /// </summary>
    public bool IsUmamiEnabled => _config.Umami.Enabled && !string.IsNullOrEmpty(_config.Umami.WebsiteId);

    /// <summary>
    /// 检查是否启用了 Clarity
    /// </summary>
    public bool IsClarityEnabled => _config.Clarity.Enabled && !string.IsNullOrEmpty(_config.Clarity.ProjectId);
}