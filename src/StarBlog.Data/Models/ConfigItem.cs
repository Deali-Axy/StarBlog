using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// 配置项目
/// </summary>
[Table(Name = "config", OldName = "ConfigItem")]
public class ConfigItem {
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    public string Key { get; set; }
    public string Value { get; set; }
    public string? Description { get; set; }
}