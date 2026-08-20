using System.ComponentModel.DataAnnotations;

namespace StarBlog.Api.Modules.Configuration;

/// <summary>配置项对外契约。</summary>
public sealed class ConfigItemResponse {
    public int Id { get; init; }
    public required string Key { get; init; }
    public required string Value { get; init; }
    public string? Description { get; init; }
}

/// <summary>创建或更新配置项。</summary>
public sealed class ConfigItemUpsertRequest {
    [Required, StringLength(128)]
    public string Key { get; init; } = string.Empty;

    [Required]
    public string Value { get; init; } = string.Empty;

    public string? Description { get; init; }
}
