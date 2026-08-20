using StarBlog.Data;
using StarBlog.Data.Models;
using StarBlog.Content;
using StarBlog.Content.Utils;

var options = ImportOptions.Parse(args);
if (options.ShowHelp) {
    ImportOptions.PrintHelp();
    return;
}

if (string.IsNullOrWhiteSpace(options.ImportDir)) {
    Console.Error.WriteLine("缺少导入目录：请通过 --importDir 指定，或设置环境变量 STAR_BLOG_IMPORT_DIR。");
    Console.Error.WriteLine();
    ImportOptions.PrintHelp();
    Environment.ExitCode = 2;
    return;
}

var importDir = Path.GetFullPath(options.ImportDir);
if (!Directory.Exists(importDir)) {
    Console.Error.WriteLine($"导入目录不存在：{importDir}");
    Environment.ExitCode = 2;
    return;
}

var repoRoot = RepoRootLocator.FindFrom(AppContext.BaseDirectory);
if (repoRoot == null) {
    Console.Error.WriteLine("无法定位仓库根目录（未找到 StarBlog.slnx）。请在仓库内运行，或通过参数显式指定路径。");
    Environment.ExitCode = 2;
    return;
}

var webDir = Path.Combine(repoRoot, "apps", "web-legacy");
var assetsPath = options.AssetsPath;
if (string.IsNullOrWhiteSpace(assetsPath)) {
    assetsPath = Path.Combine(webDir, "wwwroot", "media", "blog");
}
assetsPath = Path.GetFullPath(assetsPath);

var destDbPath = options.DestDbPath;
if (string.IsNullOrWhiteSpace(destDbPath)) {
    destDbPath = Path.Combine(webDir, "app.db");
}
destDbPath = Path.GetFullPath(destDbPath);

var localDbPath = options.LocalDbPath;
if (string.IsNullOrWhiteSpace(localDbPath)) {
    localDbPath = Path.Combine(Directory.GetCurrentDirectory(), "app.db");
}
localDbPath = Path.GetFullPath(localDbPath);

var exclusionDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".git", "logseq", "pages" };

// 删除旧文件（导入工具会在当前工作目录生成 app.db 及 wal/shm）
DeleteIfExists(localDbPath);
DeleteIfExists($"{localDbPath}-shm");
DeleteIfExists($"{localDbPath}-wal");

// 数据库（FreeSql）
var freeSql = FreeSqlFactory.Create($"Data Source={localDbPath};Synchronous=Off;Cache Size=5000;");
var postRepo = freeSql.GetRepository<Post>();
var categoryRepo = freeSql.GetRepository<Category>();

// 数据导入
WalkDirectoryTree(new DirectoryInfo(importDir));

// 复制数据库到旧 Web 项目目录（默认：apps/web-legacy/app.db）
if (File.Exists(localDbPath)) {
    Directory.CreateDirectory(Path.GetDirectoryName(destDbPath)!);
    Console.WriteLine($"复制数据库：{localDbPath} -> {destDbPath}");
    File.Copy(localDbPath, destDbPath, true);
}

return;

void DeleteIfExists(string path) {
    if (!File.Exists(path)) return;
    Console.WriteLine($"删除旧文件：{path}");
    File.Delete(path);
}

void WalkDirectoryTree(DirectoryInfo root) {
    // 参考资料：https://docs.microsoft.com/zh-cn/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree
    Console.WriteLine($"正在扫描文件夹：{root.FullName}");

    FileInfo[]? files = null;
    DirectoryInfo[]? subDirs = null;

    try {
        files = root.GetFiles("*.md");
    }
    catch (UnauthorizedAccessException e) {
        Console.WriteLine(e.Message);
    }
    catch (DirectoryNotFoundException e) {
        Console.WriteLine(e.Message);
    }

    if (files != null) {
        foreach (var fi in files) {
            Console.WriteLine(fi.FullName);

            var relativeDir = Path.GetRelativePath(importDir, fi.DirectoryName!);
            if (relativeDir == ".") relativeDir = string.Empty;

            var categoryNames = relativeDir.Split(
                new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries
            );

            Console.WriteLine($"categoryNames: {string.Join(",", categoryNames)}");

            var categories = EnsureCategories(categoryNames);

            var content = File.ReadAllText(fi.FullName);

            var post = new Post {
                Id = GuidUtils.GuidTo16String(),
                Status = "已发布",
                Title = Path.GetFileNameWithoutExtension(fi.Name),
                IsPublish = true,
                Content = content,
                Path = relativeDir,
                CreationTime = fi.CreationTime,
                LastUpdateTime = fi.LastWriteTime,
                CategoryId = categories[^1].Id,
                Categories = string.Join(",", categories.Select(a => a.Id))
            };

            var processor = new PostProcessor(importDir, assetsPath, post);

            // 处理文章标题和状态
            processor.InflateStatusTitle();

            // 处理文章正文内容：导入图片并替换图片相对路径
            post.Content = processor.MarkdownParse();
            post.Summary = processor.GetSummary(200);

            postRepo.Insert(post);
        }
    }

    subDirs = root.GetDirectories();
    if (subDirs == null) return;

    foreach (var dirInfo in subDirs) {
        if (exclusionDirs.Contains(dirInfo.Name)) continue;
        if (dirInfo.Name.EndsWith(".assets", StringComparison.OrdinalIgnoreCase)) continue;
        WalkDirectoryTree(dirInfo);
    }
}

List<Category> EnsureCategories(string[] categoryNames) {
    if (categoryNames.Length == 0) {
        var uncategorized = categoryRepo.Where(a => a.Name == "未分类").First()
                            ?? categoryRepo.Insert(new Category { Name = "未分类" });
        return new List<Category> { uncategorized };
    }

    var categories = new List<Category>();
    var rootCategory = categoryRepo.Where(a => a.Name == categoryNames[0]).First()
                       ?? categoryRepo.Insert(new Category { Name = categoryNames[0] });
    categories.Add(rootCategory);
    Console.WriteLine($"+ 添加分类: {rootCategory.Id}.{rootCategory.Name}");

    for (var i = 1; i < categoryNames.Length; i++) {
        var name = categoryNames[i];
        var parent = categories[i - 1];
        var category = categoryRepo.Where(a => a.ParentId == parent.Id && a.Name == name).First()
                       ?? categoryRepo.Insert(new Category { Name = name, ParentId = parent.Id });
        categories.Add(category);
        Console.WriteLine($"+ 添加子分类：{category.Id}.{category.Name}");
    }

    return categories;
}

internal sealed record ImportOptions(
    bool ShowHelp,
    string? ImportDir,
    string? AssetsPath,
    string? LocalDbPath,
    string? DestDbPath
) {
    public static ImportOptions Parse(string[] args) {
        var showHelp = args.Any(a => a is "--help" or "-h" or "/?");
        var importDir = GetArg(args, "--importDir") ?? Environment.GetEnvironmentVariable("STAR_BLOG_IMPORT_DIR");
        var assetsPath = GetArg(args, "--assetsPath");
        var localDbPath = GetArg(args, "--localDb");
        var destDbPath = GetArg(args, "--destDb");

        return new ImportOptions(
            ShowHelp: showHelp,
            ImportDir: importDir,
            AssetsPath: assetsPath,
            LocalDbPath: localDbPath,
            DestDbPath: destDbPath
        );
    }

    public static void PrintHelp() {
        Console.WriteLine("MarkdownImporter - StarBlog 博客文章导入工具");
        Console.WriteLine();
        Console.WriteLine("用法：");
        Console.WriteLine("  dotnet run --project tools/MarkdownImporter -- --importDir <目录> [options]");
        Console.WriteLine();
        Console.WriteLine("必填：");
        Console.WriteLine("  --importDir <目录>      Markdown 根目录（也可用环境变量 STAR_BLOG_IMPORT_DIR）");
        Console.WriteLine();
        Console.WriteLine("可选：");
        Console.WriteLine("  --assetsPath <目录>     图片导入目录（默认：apps/web-legacy/wwwroot/media/blog）");
        Console.WriteLine("  --localDb <文件>        生成的 SQLite DB 路径（默认：./app.db）");
        Console.WriteLine("  --destDb <文件>         复制到 Web 的 DB 路径（默认：apps/web-legacy/app.db）");
        Console.WriteLine();
    }

    private static string? GetArg(string[] args, string key) {
        for (var i = 0; i < args.Length; i++) {
            var current = args[i];
            if (string.Equals(current, key, StringComparison.OrdinalIgnoreCase)) {
                if (i + 1 >= args.Length) return null;
                return args[i + 1];
            }

            if (current.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase)) {
                return current[(key.Length + 1)..];
            }
        }

        return null;
    }
}

internal static class RepoRootLocator {
    public static string? FindFrom(string startPath) {
        var dir = new DirectoryInfo(startPath);
        while (dir != null) {
            if (File.Exists(Path.Combine(dir.FullName, "StarBlog.slnx"))) {
                return dir.FullName;
            }
            dir = dir.Parent;
        }

        return null;
    }
}
