namespace StarBlog.Api.Infrastructure.Storage;

/// <summary>
/// 本地或远程文件存储的可替换边界。
/// </summary>
public interface IFileStorage {
    Task EnsureDirectoryAsync(string relativeDirectory, CancellationToken cancellationToken = default);
    Task SaveAsync(string relativePath, Stream content, bool overwrite = true, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListFilesAsync(string relativeDirectory, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
}
