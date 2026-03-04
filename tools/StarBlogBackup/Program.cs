using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Data.Sqlite;

var exitCode = await new App(args).RunAsync();
Environment.Exit(exitCode);

internal sealed class App(string[] args) {
    private readonly string[] _args = args;

    public async Task<int> RunAsync() {
        if (_args.Length == 0 || IsHelp(_args[0])) {
            PrintHelp();
            return 2;
        }

        var command = _args[0].Trim();
        var options = OptionParser.Parse(_args.Skip(1).ToArray());

        return command switch {
            "backup" => await RunBackupAsync(options),
            "restore" => await RunRestoreAsync(options),
            _ => UnknownCommand(command)
        };
    }

    private static bool IsHelp(string token) => token is "-h" or "--help" or "help" or "/?";

    private static int UnknownCommand(string command) {
        Console.Error.WriteLine($"未知命令：{command}");
        Console.Error.WriteLine("可用命令：backup | restore");
        return 2;
    }

    private static void PrintHelp() {
        Console.WriteLine(
            """
            StarBlog.BackupTool

            命令：
              backup   备份 StarBlog.Web（SQLite + 媒体目录）
              restore  从备份包恢复到 StarBlog.Web（需要 --overwrite）

            backup 参数：
              --webRoot <path>        StarBlog.Web 目录（默认：当前目录下 src/StarBlog.Web）
              --outputRoot <path>     备份输出目录（默认：当前目录下 backups/StarBlog.Web）
              --retention <n>         保留最近 n 份备份（默认：30；0 表示不清理）
              --no-zip                不生成 zip，只输出目录
              --includeLogDb <bool>   是否包含 app.log.db（默认：true）

            restore 参数：
              --webRoot <path>        StarBlog.Web 目录（默认：当前目录下 src/StarBlog.Web）
              --input <path>          备份 zip 文件或备份目录
              --overwrite             允许覆盖现有文件（强制要求）

            示例：
              dotnet run --project tools/StarBlogBackup/StarBlog.BackupTool.csproj -- backup
              dotnet run --project tools/StarBlogBackup/StarBlog.BackupTool.csproj -- backup --outputRoot D:\Backups\StarBlog --retention 14
              dotnet run --project tools/StarBlogBackup/StarBlog.BackupTool.csproj -- restore --input D:\Backups\StarBlog\StarBlog.Web_20260212_120000.zip --overwrite
            """);
    }

    private static string GetDefaultWebRoot() {
        var cwd = Directory.GetCurrentDirectory();
        return Path.Combine(cwd, "src", "StarBlog.Web");
    }

    private static string GetDefaultOutputRoot() {
        var cwd = Directory.GetCurrentDirectory();
        return Path.Combine(cwd, "backups", "StarBlog.Web");
    }

    private static async Task<int> RunBackupAsync(OptionValues options) {
        var webRoot = Path.GetFullPath(options.GetString("webRoot") ?? GetDefaultWebRoot());
        var outputRoot = Path.GetFullPath(options.GetString("outputRoot") ?? GetDefaultOutputRoot());
        var retention = options.GetInt("retention") ?? 30;
        var zip = !options.HasFlag("no-zip");
        var includeLogDb = options.GetBool("includeLogDb") ?? true;

        if (!Directory.Exists(webRoot)) {
            Console.Error.WriteLine($"找不到 StarBlog.Web 目录：{webRoot}");
            return 3;
        }

        Directory.CreateDirectory(outputRoot);

        var timestamp = DateTimeOffset.Now.ToString("yyyyMMdd_HHmmss");
        var backupName = $"StarBlog.Web_{timestamp}";
        var tempRoot = Path.Combine(outputRoot,  $".tmp_{backupName}_{Guid.NewGuid():N}");
        var payloadRoot = Path.Combine(tempRoot, "StarBlog.Web");
        Directory.CreateDirectory(payloadRoot);

        try {
            var items = new List<BackupItem>();

            var dbSourcePath = Path.Combine(webRoot,     "app.db");
            var dbTargetPath = Path.Combine(payloadRoot, "app.db");
            if (!File.Exists(dbSourcePath)) {
                Console.Error.WriteLine($"找不到数据库文件：{dbSourcePath}");
                return 4;
            }

            Console.WriteLine($"备份数据库：{dbSourcePath}");
            BackupSqliteDatabase(dbSourcePath, dbTargetPath);
            items.Add(BackupItem.FromFile(dbTargetPath, "app.db"));

            var logDbSourcePath = Path.Combine(webRoot, "app.log.db");
            if (includeLogDb && File.Exists(logDbSourcePath)) {
                var logDbTargetPath = Path.Combine(payloadRoot, "app.log.db");
                Console.WriteLine($"备份日志数据库：{logDbSourcePath}");
                BackupSqliteDatabase(logDbSourcePath, logDbTargetPath);
                items.Add(BackupItem.FromFile(logDbTargetPath, "app.log.db"));
            }

            var mediaBlogSourcePath = Path.Combine(webRoot, "wwwroot", "media", "blog");
            if (Directory.Exists(mediaBlogSourcePath)) {
                var mediaBlogTargetPath = Path.Combine(payloadRoot, "wwwroot", "media", "blog");
                Console.WriteLine($"备份媒体目录：{mediaBlogSourcePath}");
                var copied = await CopyDirectoryAsync(mediaBlogSourcePath, mediaBlogTargetPath);
                items.AddRange(copied.Select(x => x with { LogicalPath = Path.Combine("wwwroot", "media", "blog", x.LogicalPath) }));
            }

            var mediaPhotoSourcePath = Path.Combine(webRoot, "wwwroot", "media", "photography");
            if (Directory.Exists(mediaPhotoSourcePath)) {
                var mediaPhotoTargetPath = Path.Combine(payloadRoot, "wwwroot", "media", "photography");
                Console.WriteLine($"备份媒体目录：{mediaPhotoSourcePath}");
                var copied = await CopyDirectoryAsync(mediaPhotoSourcePath, mediaPhotoTargetPath);
                items.AddRange(copied.Select(x => x with { LogicalPath = Path.Combine("wwwroot", "media", "photography", x.LogicalPath) }));
            }

            var manifestPath = Path.Combine(payloadRoot, "backup.manifest.json");
            var manifest = BackupManifest.Create(
                createdAt: DateTimeOffset.Now,
                webRoot: webRoot,
                items: items,
                toolVersion: typeof(App).Assembly.GetName().Version?.ToString() ?? "unknown");
            await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, BackupManifest.SerializerOptions));

            var output = zip
                ? Path.Combine(outputRoot, $"{backupName}.zip")
                : Path.Combine(outputRoot, backupName);

            if (zip) {
                if (File.Exists(output)) {
                    File.Delete(output);
                }

                ZipFile.CreateFromDirectory(tempRoot, output, CompressionLevel.Optimal, includeBaseDirectory: false);
                Directory.Delete(tempRoot, recursive: true);
            }
            else {
                if (Directory.Exists(output)) {
                    Directory.Delete(output, recursive: true);
                }

                Directory.Move(tempRoot, output);
            }

            Console.WriteLine($"备份完成：{output}");

            if (retention > 0) {
                ApplyRetention(outputRoot, retention);
            }

            return 0;
        } catch (Exception ex) {
            Console.Error.WriteLine(ex.ToString());
            try {
                if (Directory.Exists(tempRoot)) {
                    Directory.Delete(tempRoot, recursive: true);
                }
            } catch {}
            return 1;
        }
    }

    private static async Task<int> RunRestoreAsync(OptionValues options) {
        var webRoot = Path.GetFullPath(options.GetString("webRoot") ?? GetDefaultWebRoot());
        var input = options.GetString("input");
        var overwrite = options.HasFlag("overwrite");

        if (!overwrite) {
            Console.Error.WriteLine("restore 必须显式传入 --overwrite 才会执行覆盖操作。");
            return 2;
        }

        if (string.IsNullOrWhiteSpace(input)) {
            Console.Error.WriteLine("restore 必须提供 --input <path>（zip 或目录）。");
            return 2;
        }

        if (!Directory.Exists(webRoot)) {
            Console.Error.WriteLine($"找不到 StarBlog.Web 目录：{webRoot}");
            return 3;
        }

        var inputPath = Path.GetFullPath(input);
        var tempDir = Path.Combine(Path.GetTempPath(), $"StarBlogRestore_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try {
            string payloadRoot;
            if (File.Exists(inputPath) && Path.GetExtension(inputPath).Equals(".zip", StringComparison.OrdinalIgnoreCase)) {
                ZipFile.ExtractToDirectory(inputPath, tempDir);
                payloadRoot = Path.Combine(tempDir, "StarBlog.Web");
            }
            else if (Directory.Exists(inputPath)) {
                payloadRoot = Path.Combine(inputPath, "StarBlog.Web");
                if (!Directory.Exists(payloadRoot)) {
                    payloadRoot = inputPath;
                }
            }
            else {
                Console.Error.WriteLine($"找不到输入：{inputPath}");
                return 3;
            }

            if (!Directory.Exists(payloadRoot)) {
                Console.Error.WriteLine($"输入中未找到 StarBlog.Web 备份内容：{payloadRoot}");
                return 4;
            }

            Console.WriteLine($"恢复到：{webRoot}");
            await CopyDirectoryIntoAsync(payloadRoot, webRoot, overwrite: true);
            Console.WriteLine("恢复完成。建议在启动站点前确认 app.db 与媒体目录已正确覆盖。");
            return 0;
        } catch (Exception ex) {
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
        finally {
            try {
                if (Directory.Exists(tempDir)) {
                    Directory.Delete(tempDir, recursive: true);
                }
            } catch {}
        }
    }

    private static void BackupSqliteDatabase(string sourcePath, string destinationPath) {
        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
        if (File.Exists(destinationPath)) {
            File.Delete(destinationPath);
        }

        var sourceConnectionString = new SqliteConnectionStringBuilder {
            DataSource = sourcePath,
            Mode = SqliteOpenMode.ReadOnly,
            Cache = SqliteCacheMode.Shared,
            Pooling = false
        }.ToString();

        var destinationConnectionString = new SqliteConnectionStringBuilder {
            DataSource = destinationPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            Pooling = false
        }.ToString();

        using var source = new SqliteConnection(sourceConnectionString);
        using var destination = new SqliteConnection(destinationConnectionString);

        source.Open();
        destination.Open();

        source.BackupDatabase(destination);
        destination.Close();
        source.Close();
        SqliteConnection.ClearAllPools();
    }

    private static async Task<List<BackupItem>> CopyDirectoryAsync(string sourceDir, string destinationDir) {
        var result = new List<BackupItem>();
        Directory.CreateDirectory(destinationDir);

        foreach (var dir in Directory.EnumerateDirectories(sourceDir, "*", SearchOption.AllDirectories)) {
            var relativeDir = Path.GetRelativePath(sourceDir, dir);
            Directory.CreateDirectory(Path.Combine(destinationDir, relativeDir));
        }

        foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories)) {
            var relativePath = Path.GetRelativePath(sourceDir, file);
            var targetPath = Path.Combine(destinationDir, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            File.Copy(file, targetPath, overwrite: true);
            result.Add(BackupItem.FromFile(targetPath, relativePath));
        }

        return await Task.FromResult(result);
    }

    private static async Task CopyDirectoryIntoAsync(string sourceDir, string destinationDir, bool overwrite) {
        foreach (var dir in Directory.EnumerateDirectories(sourceDir, "*", SearchOption.AllDirectories)) {
            var relativeDir = Path.GetRelativePath(sourceDir, dir);
            Directory.CreateDirectory(Path.Combine(destinationDir, relativeDir));
        }

        foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories)) {
            var relativePath = Path.GetRelativePath(sourceDir, file);
            var targetPath = Path.Combine(destinationDir, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

            if (!overwrite && File.Exists(targetPath)) {
                continue;
            }

            File.Copy(file, targetPath, overwrite: true);
        }

        await Task.CompletedTask;
    }

    private static void ApplyRetention(string outputRoot, int retention) {
        var candidates = new List<FileSystemInfo>();

        candidates.AddRange(new DirectoryInfo(outputRoot).EnumerateFiles("StarBlog.Web_*.zip", SearchOption.TopDirectoryOnly));
        candidates.AddRange(new DirectoryInfo(outputRoot).EnumerateDirectories("StarBlog.Web_*", SearchOption.TopDirectoryOnly));

        var ordered = candidates
                      .Where(x => !x.Name.StartsWith(".tmp_", StringComparison.OrdinalIgnoreCase))
                      .OrderByDescending(x => x.CreationTimeUtc)
                      .ToList();

        foreach (var item in ordered.Skip(retention)) {
            try {
                switch (item) {
                    case FileInfo f when f.Exists:
                        f.Delete();
                        break;
                    case DirectoryInfo d when d.Exists:
                        d.Delete(recursive: true);
                        break;
                }
            } catch {}
        }
    }
}

internal static class OptionParser {
    public static OptionValues Parse(string[] args) {
        var dict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < args.Length; i++) {
            var token = args[i];
            if (!token.StartsWith("--", StringComparison.Ordinal)) {
                continue;
            }

            var key = token[2..].Trim();
            if (key.Length == 0) {
                continue;
            }

            string? value = null;
            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal)) {
                value = args[i + 1];
                i++;
            }

            if (!dict.TryGetValue(key, out var list)) {
                list = [];
                dict[key] = list;
            }

            list.Add(value ?? "true");
        }

        return new OptionValues(dict);
    }
}

internal readonly record struct OptionValues(IReadOnlyDictionary<string, List<string>> Values) {
    public string? GetString(string key)
        => Values.TryGetValue(key, out var list) && list.Count > 0 ? list[^1] : null;

    public bool HasFlag(string key)
        => Values.TryGetValue(key, out var list) && list.Count > 0 && list[^1].Equals("true", StringComparison.OrdinalIgnoreCase);

    public int? GetInt(string key) {
        var s = GetString(key);
        return int.TryParse(s, out var n) ? n : null;
    }

    public bool? GetBool(string key) {
        var s = GetString(key);
        return bool.TryParse(s, out var b) ? b : null;
    }
}

internal sealed record BackupItem(string LogicalPath, long SizeBytes, string Sha256) {
    public static BackupItem FromFile(string physicalPath, string logicalPath) {
        var fi = new FileInfo(physicalPath);
        using var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var hashBytes = SHA256.HashData(stream);
        var sha = Convert.ToHexString(hashBytes).ToLowerInvariant();
        return new BackupItem(logicalPath.Replace('\\', '/'), fi.Length, sha);
    }
}

internal sealed record BackupManifest(
    DateTimeOffset CreatedAt,
    string WebRoot,
    string ToolVersion,
    int ItemCount,
    long TotalSizeBytes,
    IReadOnlyList<BackupItem> Items) {
    public static BackupManifest Create(DateTimeOffset createdAt, string webRoot, IReadOnlyList<BackupItem> items, string toolVersion) {
        var total = items.Sum(x => x.SizeBytes);
        return new BackupManifest(createdAt, webRoot, toolVersion, items.Count, total, items);
    }

    public static readonly JsonSerializerOptions SerializerOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}