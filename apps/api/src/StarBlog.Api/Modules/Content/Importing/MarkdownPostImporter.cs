using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Infrastructure.Storage;
using StarBlog.Api.Infrastructure.Time;
using StarBlog.Api.Modules.Content.Domain;

namespace StarBlog.Api.Modules.Content;

/// <summary>
/// 从 Markdown 目录或 zip 导入文章：文件夹映射分类，同目录图片写入 media/blog/{postId}/。
/// 解压使用临时目录，成功或失败都会删除，避免残留文件。
/// </summary>
public sealed class MarkdownPostImporter {
    private static readonly HashSet<string> ExcludedDirectories = new(StringComparer.OrdinalIgnoreCase) {
        ".git", "logseq", "pages"
    };

    private static readonly Regex ImagePattern = new(@"!\[[^\]]*\]\(([^)]+)\)", RegexOptions.Compiled);

    private readonly StarBlogDbContext _db;
    private readonly IFileStorage _storage;
    private readonly IClock _clock;

    public MarkdownPostImporter(StarBlogDbContext db, IFileStorage storage, IClock clock) {
        _db = db;
        _storage = storage;
        _clock = clock;
    }

    /// <summary>解压 zip 到临时目录后导入，最后无论成败都清理临时文件。</summary>
    public async Task<MarkdownImportResult> ImportZipAsync(Stream zipStream, CancellationToken cancellationToken) {
        var temp = Path.Combine(Path.GetTempPath(), "starblog-import", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try {
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);
            ExtractZipSafely(archive, temp);
            return await ImportDirectoryAsync(temp, cancellationToken);
        }
        finally {
            TryDeleteDirectory(temp);
        }
    }

    /// <summary>扫描目录中的 Markdown 文件并写入文章与分类。</summary>
    public async Task<MarkdownImportResult> ImportDirectoryAsync(string root, CancellationToken cancellationToken) {
        var rootFull = Path.GetFullPath(root);
        var markdownFiles = Directory.GetFiles(rootFull, "*.md", SearchOption.AllDirectories)
            .Where(path => !IsExcluded(rootFull, path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var postIds = new List<string>();
        var categoriesCreated = 0;
        foreach (var file in markdownFiles) {
            var relativeDir = Path.GetRelativePath(rootFull, Path.GetDirectoryName(file)!);
            if (relativeDir == ".") relativeDir = string.Empty;
            var categoryNames = relativeDir.Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries);

            var (category, created) = await EnsureCategoryPathAsync(categoryNames, cancellationToken);
            categoriesCreated += created;

            var raw = await File.ReadAllTextAsync(file, cancellationToken);
            var (title, body) = ExtractTitle(raw, Path.GetFileNameWithoutExtension(file));
            var postId = Guid.NewGuid().ToString("N")[..16];
            var rewritten = await RewriteImagesAsync(body, Path.GetDirectoryName(file)!, rootFull, postId, cancellationToken);
            var info = new FileInfo(file);
            var post = new Post {
                Id = postId,
                Title = title,
                Status = "已发布",
                IsPublish = true,
                Content = rewritten,
                Summary = BuildSummary(rewritten),
                Path = relativeDir.Replace('\\', '/'),
                CategoryId = category.Id,
                CreationTime = info.CreationTime,
                LastUpdateTime = info.LastWriteTime == default ? _clock.Now : info.LastWriteTime
            };
            _db.Posts.Add(post);
            await _db.SaveChangesAsync(cancellationToken);
            postIds.Add(postId);
        }

        return new MarkdownImportResult {
            PostsImported = postIds.Count,
            CategoriesCreated = categoriesCreated,
            PostIds = postIds
        };
    }

    /// <summary>按相对路径逐级创建或复用分类。</summary>
    private async Task<(Category Category, int Created)> EnsureCategoryPathAsync(string[] names, CancellationToken cancellationToken) {
        if (names.Length == 0) {
            var uncategorized = await _db.Categories.FirstOrDefaultAsync(item => item.Name == "未分类" && item.ParentId == 0, cancellationToken);
            if (uncategorized != null) return (uncategorized, 0);
            uncategorized = new Category { Name = "未分类", ParentId = 0, Visible = true };
            _db.Categories.Add(uncategorized);
            await _db.SaveChangesAsync(cancellationToken);
            return (uncategorized, 1);
        }

        Category? current = null;
        var parentId = 0;
        var created = 0;
        foreach (var name in names) {
            current = await _db.Categories.FirstOrDefaultAsync(item => item.ParentId == parentId && item.Name == name, cancellationToken);
            if (current == null) {
                current = new Category { Name = name, ParentId = parentId, Visible = true };
                _db.Categories.Add(current);
                await _db.SaveChangesAsync(cancellationToken);
                created++;
            }

            parentId = current.Id;
        }

        return (current!, created);
    }

    /// <summary>把相对路径图片复制到文章媒体目录，并改写 Markdown 链接。</summary>
    private async Task<string> RewriteImagesAsync(string content, string markdownDir, string importRoot, string postId, CancellationToken cancellationToken) {
        var matches = ImagePattern.Matches(content);
        if (matches.Count == 0) return content;

        var updated = content;
        foreach (Match match in matches.Cast<Match>().Reverse()) {
            var rawSrc = match.Groups[1].Value.Trim().Trim('"', '\'');
            var src = rawSrc.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[0];
            if (src.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || src.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || src.StartsWith('/')) {
                continue;
            }

            var local = Path.GetFullPath(Path.Combine(markdownDir, src.Replace('/', Path.DirectorySeparatorChar)));
            if (!local.StartsWith(Path.GetFullPath(importRoot) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || !File.Exists(local)) {
                continue;
            }

            var fileName = Path.GetFileName(local);
            var relative = Path.Combine("media", "blog", postId, fileName).Replace('\\', '/');
            await using var stream = File.OpenRead(local);
            await _storage.SaveAsync(relative, stream, cancellationToken: cancellationToken);
            var replacement = match.Value.Replace(src, "/" + relative, StringComparison.Ordinal);
            updated = updated.Remove(match.Index, match.Length).Insert(match.Index, replacement);
        }

        return updated;
    }

    /// <summary>优先使用文首一级标题，否则使用文件名。</summary>
    private static (string Title, string Body) ExtractTitle(string content, string fallback) {
        using var reader = new StringReader(content);
        var first = reader.ReadLine();
        if (first != null && first.StartsWith("# ", StringComparison.Ordinal)) {
            var rest = reader.ReadToEnd();
            return (first[2..].Trim(), rest.TrimStart());
        }

        return (fallback, content);
    }

    /// <summary>去掉 Markdown 标记后截取摘要。</summary>
    private static string BuildSummary(string markdown) {
        var text = Regex.Replace(markdown, @"!\[[^\]]*\]\([^)]+\)", string.Empty);
        text = Regex.Replace(text, @"[#*_`>\-\[\]()]", string.Empty);
        text = Regex.Replace(text, @"\s+", " ").Trim();
        return text.Length <= 200 ? text : text[..200];
    }

    private static bool IsExcluded(string root, string filePath) {
        var relative = Path.GetRelativePath(root, filePath);
        var parts = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Any(part => ExcludedDirectories.Contains(part)
                                 || part.EndsWith(".assets", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>只解压落在目标目录内的条目，拒绝 zip slip。</summary>
    private static void ExtractZipSafely(ZipArchive archive, string destination) {
        var root = Path.GetFullPath(destination) + Path.DirectorySeparatorChar;
        foreach (var entry in archive.Entries) {
            var relative = entry.FullName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            if (string.IsNullOrWhiteSpace(relative)) continue;
            var dest = Path.GetFullPath(Path.Combine(destination, relative));
            if (!dest.StartsWith(root, StringComparison.OrdinalIgnoreCase)) continue;
            if (relative.EndsWith(Path.DirectorySeparatorChar)) {
                Directory.CreateDirectory(dest);
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            entry.ExtractToFile(dest, overwrite: true);
        }
    }

    private static void TryDeleteDirectory(string path) {
        try {
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }
        catch (IOException) {
        }
    }
}
