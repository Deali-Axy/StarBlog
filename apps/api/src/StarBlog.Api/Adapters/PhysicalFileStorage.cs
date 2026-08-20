using StarBlog.Application.Abstractions;

namespace StarBlog.Api.Adapters;

public sealed class PhysicalFileStorage : IFileStorage {
    private readonly IAppPathProvider _paths;

    public PhysicalFileStorage(IAppPathProvider paths) {
        _paths = paths;
    }

    public Task EnsureDirectoryAsync(string relativeDirectory, CancellationToken cancellationToken = default) {
        var dir = MapPath(relativeDirectory);
        Directory.CreateDirectory(dir);
        return Task.CompletedTask;
    }

    public async Task SaveAsync(string relativePath, Stream content, bool overwrite = true, CancellationToken cancellationToken = default) {
        var fullPath = MapPath(relativePath);
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(dir)) {
            Directory.CreateDirectory(dir);
        }

        var mode = overwrite ? FileMode.Create : FileMode.CreateNew;
        await using var fs = new FileStream(fullPath, mode, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fs, cancellationToken);
    }

    public Task<IReadOnlyList<string>> ListFilesAsync(string relativeDirectory, CancellationToken cancellationToken = default) {
        var dir = MapPath(relativeDirectory);
        if (!Directory.Exists(dir)) {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        var files = Directory.GetFiles(dir)
            .Select(f => Path.GetFileName(f)!)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
        return Task.FromResult<IReadOnlyList<string>>(files);
    }

    public Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default) {
        return Task.FromResult(File.Exists(MapPath(relativePath)));
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default) {
        var fullPath = MapPath(relativePath);
        if (File.Exists(fullPath)) {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    private string MapPath(string relativePath) {
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        if (Path.IsPathRooted(normalized)) {
            return normalized;
        }
        return Path.Combine(_paths.WebRootPath, normalized);
    }
}
