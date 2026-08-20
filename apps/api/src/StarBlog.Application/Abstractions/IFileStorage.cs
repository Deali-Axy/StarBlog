namespace StarBlog.Application.Abstractions;

public interface IFileStorage {
    Task EnsureDirectoryAsync(string relativeDirectory, CancellationToken cancellationToken = default);
    Task SaveAsync(string relativePath, Stream content, bool overwrite = true, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListFilesAsync(string relativeDirectory, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
}
