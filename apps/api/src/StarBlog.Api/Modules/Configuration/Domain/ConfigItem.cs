namespace StarBlog.Api.Modules.Configuration.Domain;

/// <summary>
/// 运行时站点配置项。
/// </summary>
public sealed class ConfigItem {
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}
