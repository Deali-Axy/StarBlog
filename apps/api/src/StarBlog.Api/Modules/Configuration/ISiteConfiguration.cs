namespace StarBlog.Api.Modules.Configuration;

/// <summary>
/// 其他模块读取运行时配置的公开契约。
/// </summary>
public interface ISiteConfiguration {
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, string value, string? description = null, CancellationToken cancellationToken = default);
}
