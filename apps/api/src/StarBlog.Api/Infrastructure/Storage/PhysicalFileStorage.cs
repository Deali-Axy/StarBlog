namespace StarBlog.Api.Infrastructure.Storage;

/// <summary>
/// 将相对路径映射到 WebRoot 下的物理文件。
/// </summary>
public sealed class PhysicalFileStorage : IFileStorage {
    private readonly IAppPathProvider _paths;

    public PhysicalFileStorage(IAppPathProvider paths) {
        _paths = paths;
    }

    /// <summary>确保相对目录存在。</summary>
    public Task EnsureDirectoryAsync(string relativeDirectory, CancellationToken cancellationToken = default) {
        Directory.CreateDirectory(MapPath(relativeDirectory));
        return Task.CompletedTask;
    }

    /// <summary>把流写入相对路径；必要时创建父目录。</summary>
    public async Task SaveAsync(string relativePath, Stream content, bool overwrite = true, CancellationToken cancellationToken = default) {
        var fullPath = MapPath(relativePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory)) {
            Directory.CreateDirectory(directory);
        }

        var mode = overwrite ? FileMode.Create : FileMode.CreateNew;
        await using var stream = new FileStream(fullPath, mode, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(stream, cancellationToken);
    }

    /// <summary>列出相对目录中的文件名；目录不存在时返回空列表。</summary>
    public Task<IReadOnlyList<string>> ListFilesAsync(string relativeDirectory, CancellationToken cancellationToken = default) {
        var directory = MapPath(relativeDirectory);
        if (!Directory.Exists(directory)) {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        var files = Directory.GetFiles(directory)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .ToArray();
        return Task.FromResult<IReadOnlyList<string>>(files);
    }

    /// <summary>判断相对路径对应的文件是否存在。</summary>
    public Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default) {
        return Task.FromResult(File.Exists(MapPath(relativePath)));
    }

    /// <summary>删除相对路径对应的文件；文件不存在时忽略。</summary>
    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default) {
        var fullPath = MapPath(relativePath);
        if (File.Exists(fullPath)) {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string MapPath(string relativePath) {
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.IsPathRooted(normalized)
            ? normalized
            : Path.Combine(_paths.WebRootPath, normalized);
    }
}
