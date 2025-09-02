// See https://aka.ms/new-console-template for more information

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

// 主程序入口
await RunProgram();

return;

// 获取目录路径的方法
static string? GetDirectoryPath() {
    Console.WriteLine("\n=== 选择目录路径 ===");

    // 显示当前工作目录
    string currentDir = Directory.GetCurrentDirectory();
    Console.WriteLine($"当前工作目录: {currentDir}");

    // 提供常用路径选项
    Console.WriteLine("\n快速选择:");
    Console.WriteLine("1. 使用当前目录");
    Console.WriteLine("2. 桌面");
    Console.WriteLine("3. 图片文件夹");
    Console.WriteLine("4. 自定义路径");
    Console.Write("请选择 (1-4) 或直接输入路径: ");

    string? input = Console.ReadLine()?.Trim();
    string? targetPath = null;

    switch (input) {
        case "1":
            targetPath = currentDir;
            break;
        case "2":
            targetPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            break;
        case "3":
            targetPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            break;
        case "4":
            Console.Write("请输入自定义路径: ");
            targetPath = Console.ReadLine()?.Trim('"');
            break;
        default:
            // 直接输入的路径
            targetPath = input?.Trim('"');
            break;
    }

    if (string.IsNullOrWhiteSpace(targetPath)) {
        Console.WriteLine("错误: 路径不能为空");
        return null;
    }

    if (!Directory.Exists(targetPath)) {
        Console.WriteLine($"错误: 目录不存在 - {targetPath}");
        Console.WriteLine("请检查路径是否正确");

        // 提供重试机会
        Console.Write("是否重新输入? (y/n): ");
        if (Console.ReadLine()?.ToLower() == "y") {
            return GetDirectoryPath();
        }
        return null;
    }

    // 显示目录信息
    try {
        var dirInfo = new DirectoryInfo(targetPath);
        var fileCount = dirInfo.GetFiles("*", SearchOption.AllDirectories).Length;
        var subDirCount = dirInfo.GetDirectories("*", SearchOption.AllDirectories).Length;

        Console.WriteLine($"\n✓ 目录路径有效: {targetPath}");
        Console.WriteLine($"  包含文件: {fileCount} 个");
        Console.WriteLine($"  包含子目录: {subDirCount} 个");
    } catch (Exception ex) {
        Console.WriteLine($"警告: 无法读取目录信息 - {ex.Message}");
    }

    return targetPath;
}

// 主程序方法
static async Task RunProgram() {
    Console.WriteLine("博客图片管理工具");
    Console.WriteLine("1. 文件概览分析");
    Console.WriteLine("2. 批量压缩图片");
    Console.WriteLine("3. 压缩单个图片");
    Console.WriteLine("请选择功能 (1-3): ");

    string choice = Console.ReadLine() ?? "1";

    // 根据选择的功能获取路径
    string? startPath = null;
    if (choice == "1" || choice == "2") {
        startPath = GetDirectoryPath();
        if (string.IsNullOrEmpty(startPath)) {
            Console.WriteLine("未提供有效路径，程序退出。");
            return;
        }
    }

    switch (choice)
    {
        case "1":
            AnalyzeFiles(startPath!);
            break;
        case "2":
            await CompressImages(startPath!);
            break;
        case "3":
            await CompressSingleImageInteractive();
            break;
        default:
            Console.WriteLine("无效选择，执行文件分析...");
            if (!string.IsNullOrEmpty(startPath)) {
                AnalyzeFiles(startPath);
            }
            break;
    }

    Console.WriteLine("\n按任意键退出...");
    Console.Read();
}

// 格式化文件大小的辅助方法
static string FormatFileSize(long bytes) {
    string[] sizes = { "B", "KB", "MB", "GB", "TB" };
    double len = bytes;
    int order = 0;
    while (len >= 1024 && order < sizes.Length - 1) {
        order++;
        len = len / 1024;
    }

    return $"{len:0.##} {sizes[order]}";
}

// 文件概览分析方法
static void AnalyzeFiles(string startPath) {
    try {
        Console.WriteLine("=== 遍历所有文件（按大小排序）===");
        // 使用推荐方法：EnumerateFiles 配合 SearchOption.AllDirectories
        var files = Directory.EnumerateFiles(startPath, "*", SearchOption.AllDirectories);

        // 获取文件信息并按大小排序
        var fileInfos = files
            .Select(file => new FileInfo(file))
            .Where(fi => fi.Exists) // 确保文件存在
            .OrderByDescending(fi => fi.Length) // 按大小降序排列
            .ToList();

        foreach (var fileInfo in fileInfos) {
            Console.WriteLine($"File: {fileInfo.FullName}");
            Console.WriteLine($"      Size: {FormatFileSize(fileInfo.Length)} ({fileInfo.Length:N0} bytes)");
            Console.WriteLine($"      Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();
        }

        Console.WriteLine("\n=== 遍历所有目录 ===");
        // 递归获取所有子目录
        var directories = Directory.EnumerateDirectories(startPath, "*", SearchOption.AllDirectories);
        foreach (string directory in directories) {
            Console.WriteLine($"Directory: {directory}");
        }

        Console.WriteLine("\n=== 文件大小分析 ===");
        if (fileInfos.Any()) {
            long totalSize = fileInfos.Sum(fi => fi.Length);
            long maxSize = fileInfos.Max(fi => fi.Length);
            long minSize = fileInfos.Min(fi => fi.Length);
            double avgSize = fileInfos.Average(fi => fi.Length);

            Console.WriteLine($"总文件大小: {FormatFileSize(totalSize)} ({totalSize:N0} bytes)");
            Console.WriteLine($"最大文件: {FormatFileSize(maxSize)} - {fileInfos.First(fi => fi.Length == maxSize).Name}");
            Console.WriteLine($"最小文件: {FormatFileSize(minSize)} - {fileInfos.First(fi => fi.Length == minSize).Name}");
            Console.WriteLine($"平均文件大小: {FormatFileSize((long)avgSize)}");

            // 文件大小分布
            var sizeRanges = new[] {
                (name: "< 1KB", min: 0L, max: 1024L),
                (name: "1KB - 100KB", min: 1024L, max: 100 * 1024L),
                (name: "100KB - 1MB", min: 100 * 1024L, max: 1024 * 1024L),
                (name: "1MB - 10MB", min: 1024 * 1024L, max: 10 * 1024 * 1024L),
                (name: "> 10MB", min: 10 * 1024 * 1024L, max: long.MaxValue)
            };

            Console.WriteLine("\n文件大小分布:");
            foreach (var range in sizeRanges) {
                int count = fileInfos.Count(fi => fi.Length >= range.min && fi.Length < range.max);
                if (count > 0) {
                    long rangeSize = fileInfos
                        .Where(fi => fi.Length >= range.min && fi.Length < range.max)
                        .Sum(fi => fi.Length);
                    Console.WriteLine($"  {range.name}: {count} 个文件, 总大小: {FormatFileSize(rangeSize)}");
                }
            }

            // 显示前5个最大的文件
            Console.WriteLine("\n前5个最大的文件:");
            foreach (var fileInfo in fileInfos.Take(5)) {
                Console.WriteLine($"  {fileInfo.Name}: {FormatFileSize(fileInfo.Length)}");
            }
        }

        Console.WriteLine("\n=== 统计信息 ===");
        int fileCount = fileInfos.Count;
        int dirCount = Directory.EnumerateDirectories(startPath, "*", SearchOption.AllDirectories).Count();
        Console.WriteLine($"总文件数: {fileCount}");
        Console.WriteLine($"总目录数: {dirCount}");
    }
    catch (UnauthorizedAccessException ex) {
        Console.WriteLine($"访问被拒绝: {ex.Message}");
    }
    catch (DirectoryNotFoundException ex) {
        Console.WriteLine($"目录不存在: {ex.Message}");
    }
    catch (Exception ex) {
        Console.WriteLine($"发生错误: {ex.Message}");
    }
}

// 图片压缩方法
static async Task CompressImages(string startPath) {
    try {
        Console.WriteLine("=== 博客图片压缩工具 ===");

        // 支持的图片格式
        string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".gif" };

        // 获取所有图片文件
        var imageFiles = Directory.EnumerateFiles(startPath, "*", SearchOption.AllDirectories)
            .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()))
            .ToList();

        if (!imageFiles.Any()) {
            Console.WriteLine("未找到图片文件！");
            return;
        }

        Console.WriteLine($"找到 {imageFiles.Count} 个图片文件");
        Console.WriteLine("\n压缩配置（博客最佳实践）:");
        Console.WriteLine("- 文章配图: 1200x800px, 质量85%, WebP格式");
        Console.WriteLine("- 缩略图: 400x300px, 质量80%, WebP格式");
        Console.WriteLine("- 头图/横幅: 1920x1080px, 质量90%, WebP格式");

        // 检查是否包含GIF文件
        var gifFiles = imageFiles.Where(f => Path.GetExtension(f).ToLower() == ".gif").ToList();
        var otherFiles = imageFiles.Where(f => Path.GetExtension(f).ToLower() != ".gif").ToList();

        if (gifFiles.Any()) {
            Console.WriteLine($"\n发现 {gifFiles.Count} 个GIF文件，{otherFiles.Count} 个其他格式文件");
            Console.WriteLine("GIF文件将使用专门的优化策略");
        }

        Console.WriteLine("\n选择压缩模式:");
        Console.WriteLine("1. 文章配图模式 (1200x800, 质量85% | GIF: 800x600, 64色)");
        Console.WriteLine("2. 缩略图模式 (400x300, 质量80% | GIF: 400x300, 32色)");
        Console.WriteLine("3. 头图模式 (1920x1080, 质量90% | GIF: 1200x900, 128色)");
        Console.WriteLine("4. 自定义模式");
        Console.Write("请选择 (1-4): ");

        string mode = Console.ReadLine() ?? "1";
        var config = GetCompressionConfig(mode);

        // 创建输出目录
        string outputDir = Path.Combine(startPath, "compressed");
        Directory.CreateDirectory(outputDir);

        Console.WriteLine($"\n开始压缩，输出目录: {outputDir}");
        Console.WriteLine("压缩进度:");

        int processed = 0;
        long originalTotalSize = 0;
        long compressedTotalSize = 0;

        foreach (var imagePath in imageFiles) {
            try {
                var originalInfo = new FileInfo(imagePath);
                originalTotalSize += originalInfo.Length;

                string fileName = Path.GetFileNameWithoutExtension(imagePath);
                string extension = Path.GetExtension(imagePath).ToLower();
                string outputPath;

                // 根据文件类型选择输出路径和处理方式
                if (extension == ".gif") {
                    outputPath = Path.Combine(outputDir, $"{fileName}_compressed.gif");

                    // 分析GIF并使用专门的压缩方法
                    using var image = await Image.LoadAsync(imagePath);
                    var (gifType, frameCount, hasTransparency) = AnalyzeGif(imagePath);
                    var gifConfig = GetGifCompressionConfig(mode, gifType, image.Width, image.Height);

                    await CompressGifImage(imagePath, outputPath, gifConfig);
                } else {
                    outputPath = Path.Combine(outputDir, $"{fileName}_compressed.webp");
                    await CompressSingleImage(imagePath, outputPath, config);
                }

                var compressedInfo = new FileInfo(outputPath);
                compressedTotalSize += compressedInfo.Length;

                processed++;
                double progress = (double)processed / imageFiles.Count * 100;

                Console.WriteLine($"[{progress:F1}%] {originalInfo.Name} -> {compressedInfo.Name}");
                Console.WriteLine($"  原始: {FormatFileSize(originalInfo.Length)} -> 压缩: {FormatFileSize(compressedInfo.Length)} " +
                                $"(节省 {FormatFileSize(originalInfo.Length - compressedInfo.Length)})");

            } catch (Exception ex) {
                Console.WriteLine($"压缩失败 {Path.GetFileName(imagePath)}: {ex.Message}");
            }
        }

        // 压缩总结
        Console.WriteLine("\n=== 压缩完成 ===");
        Console.WriteLine($"处理文件: {processed}/{imageFiles.Count}");
        Console.WriteLine($"原始总大小: {FormatFileSize(originalTotalSize)}");
        Console.WriteLine($"压缩后总大小: {FormatFileSize(compressedTotalSize)}");
        Console.WriteLine($"总节省空间: {FormatFileSize(originalTotalSize - compressedTotalSize)}");
        Console.WriteLine($"压缩率: {(1 - (double)compressedTotalSize / originalTotalSize) * 100:F1}%");

    } catch (Exception ex) {
        Console.WriteLine($"压缩过程发生错误: {ex.Message}");
    }
}

// 获取压缩配置
static CompressionConfig GetCompressionConfig(string mode) {
    return mode switch {
        "1" => new CompressionConfig(1200, 800, 85, "文章配图模式"),
        "2" => new CompressionConfig(400, 300, 80, "缩略图模式"),
        "3" => new CompressionConfig(1920, 1080, 90, "头图模式"),
        "4" => GetCustomConfig(),
        _ => new CompressionConfig(1200, 800, 85, "默认文章配图模式")
    };
}

// 获取自定义配置
static CompressionConfig GetCustomConfig() {
    Console.Write("请输入目标宽度: ");
    int width = int.TryParse(Console.ReadLine(), out int w) ? w : 1200;

    Console.Write("请输入目标高度: ");
    int height = int.TryParse(Console.ReadLine(), out int h) ? h : 800;

    Console.Write("请输入质量 (1-100): ");
    int quality = int.TryParse(Console.ReadLine(), out int q) ? Math.Clamp(q, 1, 100) : 85;

    return new CompressionConfig(width, height, quality, "自定义模式");
}

// 压缩单个图片
static async Task CompressSingleImage(string inputPath, string outputPath, CompressionConfig config) {
    // 检查是否为GIF格式
    string extension = Path.GetExtension(inputPath).ToLower();
    if (extension == ".gif") {
        // 分析GIF特征
        var (gifType, frameCount, hasTransparency) = AnalyzeGif(inputPath);

        Console.WriteLine($"\n=== GIF分析结果 ===");
        Console.WriteLine($"类型: {(gifType == GifType.Animated ? "动画GIF" : "静态GIF")}");
        Console.WriteLine($"帧数: {frameCount}");
        Console.WriteLine($"透明度: {(hasTransparency ? "是" : "否")}");

        // 询问用户是否要保持GIF格式
        Console.WriteLine("\n选择处理方式:");
        Console.WriteLine("1. 优化为GIF格式 (保持动画效果)");
        Console.WriteLine("2. 转换为其他格式 (静态图片)");
        Console.Write("请选择 (1-2): ");

        string choice = Console.ReadLine() ?? "1";

        if (choice == "1") {
            // 使用GIF专用压缩
            using var image = await Image.LoadAsync(inputPath);
            var gifConfig = GetGifCompressionConfig("4", gifType, image.Width, image.Height); // 默认保持原尺寸
            await CompressGifImage(inputPath, outputPath, gifConfig);
            return;
        }
        // 如果选择转换格式，继续使用常规处理
    }

    using var regularImage = await Image.LoadAsync(inputPath);

    // 计算缩放比例，保持宽高比
    double scaleX = (double)config.Width / regularImage.Width;
    double scaleY = (double)config.Height / regularImage.Height;
    double scale = Math.Min(scaleX, scaleY);

    // 如果图片已经比目标尺寸小，则不放大
    if (scale > 1.0) {
        scale = 1.0;
    }

    int newWidth = (int)(regularImage.Width * scale);
    int newHeight = (int)(regularImage.Height * scale);

    // 应用变换
    regularImage.Mutate(x => x.Resize(newWidth, newHeight));

    // 智能选择压缩格式
    await SaveWithOptimalFormat(regularImage, outputPath, config);
}

// 交互式压缩单个图片
static async Task CompressSingleImageInteractive() {
    try {
        Console.WriteLine("=== 单个图片压缩工具 ===");

        // 获取输入文件路径
        Console.Write("请输入图片文件路径（支持拖拽）: ");
        string inputPath = Console.ReadLine()?.Trim('"') ?? "";

        if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath)) {
            Console.WriteLine("文件不存在或路径无效！");
            return;
        }

        // 检查是否为图片文件
        string[] supportedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".gif" };
        string extension = Path.GetExtension(inputPath).ToLower();

        if (!supportedExtensions.Contains(extension)) {
            Console.WriteLine($"不支持的文件格式: {extension}");
            Console.WriteLine($"支持的格式: {string.Join(", ", supportedExtensions)}");
            return;
        }

        // 显示原始图片信息
        var originalInfo = new FileInfo(inputPath);
        using var originalImage = await Image.LoadAsync(inputPath);

        Console.WriteLine($"\n原始图片信息:");
        Console.WriteLine($"  文件名: {originalInfo.Name}");
        Console.WriteLine($"  尺寸: {originalImage.Width} × {originalImage.Height} px");
        Console.WriteLine($"  文件大小: {FormatFileSize(originalInfo.Length)}");
        Console.WriteLine($"  格式: {extension.ToUpper()}");

        // 检查是否为GIF格式，提供专门的选项
        if (extension == ".gif") {
            var (gifType, frameCount, hasTransparency) = AnalyzeGif(inputPath);

            Console.WriteLine($"\n=== GIF分析结果 ===");
            Console.WriteLine($"类型: {(gifType == GifType.Animated ? "动画GIF" : "静态GIF")}");
            Console.WriteLine($"帧数: {frameCount}");
            Console.WriteLine($"透明度: {(hasTransparency ? "是" : "否")}");

            Console.WriteLine("\n选择GIF压缩模式:");
            Console.WriteLine("1. GIF博客配图模式 (800×600, 64色)");
            Console.WriteLine("2. GIF缩略图模式 (400×300, 32色)");
            Console.WriteLine("3. GIF头图模式 (1200×900, 128色)");
            Console.WriteLine("4. 保持原尺寸，仅优化颜色(64色)");
            Console.WriteLine("5. 自定义GIF模式");
            Console.WriteLine("6. 转换为其他格式 (失去动画效果)");
            Console.Write("请选择 (1-6): ");

            string gifMode = Console.ReadLine() ?? "1";

            if (gifMode != "6") {
                // 使用GIF专用压缩
                var gifConfig = GetGifCompressionConfig(gifMode, gifType, originalImage.Width, originalImage.Height);

                Console.WriteLine($"\n压缩配置: {gifConfig.Description}");
                Console.WriteLine($"目标尺寸: {gifConfig.Width} × {gifConfig.Height}");
                Console.WriteLine($"最大颜色数: {gifConfig.MaxColors}");
                Console.WriteLine($"抖动: {(gifConfig.EnableDithering ? "启用" : "禁用")}");

                // 选择输出路径
                Console.WriteLine("\n选择输出方式:");
                Console.WriteLine("1. 保存到原文件夹（添加_compressed后缀）");
                Console.WriteLine("2. 自定义输出路径");
                Console.Write("请选择 (1-2): ");

                string gifOutputChoice = Console.ReadLine() ?? "1";
                string gifOutputPath;

                if (gifOutputChoice == "2") {
                    Console.Write("请输入输出文件路径: ");
                    gifOutputPath = Console.ReadLine()?.Trim('"') ?? "";
                    if (string.IsNullOrEmpty(gifOutputPath)) {
                        gifOutputPath = GetDefaultGifOutputPath(inputPath);
                    }
                } else {
                    gifOutputPath = GetDefaultGifOutputPath(inputPath);
                }

                // 确保输出目录存在
                string gifOutputDir = Path.GetDirectoryName(gifOutputPath) ?? "";
                if (!string.IsNullOrEmpty(gifOutputDir)) {
                    Directory.CreateDirectory(gifOutputDir);
                }

                Console.WriteLine($"\n开始GIF压缩...");
                Console.WriteLine($"输出路径: {gifOutputPath}");

                // 执行GIF压缩
                await CompressGifImage(inputPath, gifOutputPath, gifConfig);

                // 显示结果
                var gifCompressedInfo = new FileInfo(gifOutputPath);

                Console.WriteLine($"\n=== GIF压缩完成 ===");
                Console.WriteLine($"原始: {FormatFileSize(originalInfo.Length)} ({originalImage.Width}×{originalImage.Height})");
                Console.WriteLine($"压缩: {FormatFileSize(gifCompressedInfo.Length)}");
                Console.WriteLine($"节省: {FormatFileSize(originalInfo.Length - gifCompressedInfo.Length)}");
                Console.WriteLine($"压缩率: {(1 - (double)gifCompressedInfo.Length / originalInfo.Length) * 100:F1}%");
                Console.WriteLine($"输出文件: {gifOutputPath}");

                return; // 直接返回，不继续常规处理
            }
            // 如果选择转换格式，继续常规处理
        }

        // 选择压缩模式（非GIF或选择转换格式的GIF）
        Console.WriteLine("\n选择压缩模式:");
        Console.WriteLine("1. 文章配图模式 (1200×800, 质量85%)");
        Console.WriteLine("2. 缩略图模式 (400×300, 质量80%)");
        Console.WriteLine("3. 头图模式 (1920×1080, 质量90%)");
        Console.WriteLine("4. 保持原尺寸，仅压缩质量(80%)");
        Console.WriteLine("5. 自定义模式");
        Console.Write("请选择 (1-5): ");

        string mode = Console.ReadLine() ?? "1";
        var config = mode switch {
            "1" => new CompressionConfig(1200, 800, 85, "文章配图模式"),
            "2" => new CompressionConfig(400, 300, 80, "缩略图模式"),
            "3" => new CompressionConfig(1920, 1080, 90, "头图模式"),
            "4" => new CompressionConfig(originalImage.Width, originalImage.Height, 80, "保持原尺寸"),
            "5" => GetCustomConfig(),
            _ => new CompressionConfig(1200, 800, 85, "默认文章配图模式")
        };

        // 选择输出路径
        Console.WriteLine($"\n压缩配置: {config.Description}");
        Console.WriteLine($"目标尺寸: {config.Width} × {config.Height} px");
        Console.WriteLine($"质量: {config.Quality}%");

        Console.WriteLine("\n选择输出方式:");
        Console.WriteLine("1. 保存到原文件夹（添加_compressed后缀）");
        Console.WriteLine("2. 自定义输出路径");
        Console.Write("请选择 (1-2): ");

        string outputChoice = Console.ReadLine() ?? "1";
        string outputPath;

        if (outputChoice == "2") {
            Console.Write("请输入输出文件路径: ");
            outputPath = Console.ReadLine()?.Trim('"') ?? "";
            if (string.IsNullOrEmpty(outputPath)) {
                outputPath = GetDefaultOutputPath(inputPath);
            }
        } else {
            outputPath = GetDefaultOutputPath(inputPath);
        }

        // 确保输出目录存在
        string outputDir = Path.GetDirectoryName(outputPath) ?? "";
        if (!string.IsNullOrEmpty(outputDir)) {
            Directory.CreateDirectory(outputDir);
        }

        Console.WriteLine($"\n开始压缩...");
        Console.WriteLine($"输出路径: {outputPath}");

        // 执行压缩
        await CompressSingleImage(inputPath, outputPath, config);

        // 显示结果
        var compressedInfo = new FileInfo(outputPath);
        using var compressedImage = await Image.LoadAsync(outputPath);

        Console.WriteLine($"\n=== 压缩完成 ===");
        Console.WriteLine($"原始: {FormatFileSize(originalInfo.Length)} ({originalImage.Width}×{originalImage.Height})");
        Console.WriteLine($"压缩: {FormatFileSize(compressedInfo.Length)} ({compressedImage.Width}×{compressedImage.Height})");
        Console.WriteLine($"节省: {FormatFileSize(originalInfo.Length - compressedInfo.Length)}");
        Console.WriteLine($"压缩率: {(1 - (double)compressedInfo.Length / originalInfo.Length) * 100:F1}%");
        Console.WriteLine($"输出文件: {outputPath}");

    } catch (Exception ex) {
        Console.WriteLine($"压缩失败: {ex.Message}");
    }
}

// 智能保存最优格式
static async Task SaveWithOptimalFormat(Image image, string outputPath, CompressionConfig config) {
    // 分析图片特征
    bool hasTransparency = HasTransparency(image);
    bool isSimpleGraphic = IsSimpleGraphic(image);

    Console.WriteLine($"\n图片分析:");
    Console.WriteLine($"  透明度: {(hasTransparency ? "是" : "否")}");
    Console.WriteLine($"  图片类型: {(isSimpleGraphic ? "图形/图标" : "照片/复杂图像")}");

    // 智能选择格式
    if (hasTransparency) {
        // 有透明度，使用 WebP
        Console.WriteLine($"  推荐格式: WebP (保持透明度)");
        var webpEncoder = new WebpEncoder {
            Quality = config.Quality,
            Method = WebpEncodingMethod.BestQuality
        };
        string webpPath = Path.ChangeExtension(outputPath, ".webp");
        await image.SaveAsync(webpPath, webpEncoder);

        // 更新输出路径
        File.Move(webpPath, outputPath);
    } else if (isSimpleGraphic) {
        // 简单图形，比较 WebP 和 PNG
        Console.WriteLine($"  推荐格式: WebP (图形优化)");
        var webpEncoder = new WebpEncoder {
            Quality = config.Quality,
            Method = WebpEncodingMethod.BestQuality
        };
        string webpPath = Path.ChangeExtension(outputPath, ".webp");
        await image.SaveAsync(webpPath, webpEncoder);
        File.Move(webpPath, outputPath);
    } else {
        // 复杂图像/照片，比较 WebP 和 JPEG
        Console.WriteLine($"  测试最优格式...");

        // 创建临时文件测试压缩效果
        string tempWebp = Path.GetTempFileName() + ".webp";
        string tempJpeg = Path.GetTempFileName() + ".jpg";

        try {
            // 测试 WebP
            var webpEncoder = new WebpEncoder {
                Quality = config.Quality,
                Method = WebpEncodingMethod.BestQuality
            };
            await image.SaveAsync(tempWebp, webpEncoder);

            // 测试 JPEG
            var jpegEncoder = new JpegEncoder {
                Quality = config.Quality
            };
            await image.SaveAsync(tempJpeg, jpegEncoder);

            // 比较文件大小
            var webpSize = new FileInfo(tempWebp).Length;
            var jpegSize = new FileInfo(tempJpeg).Length;

            Console.WriteLine($"  WebP 大小: {FormatFileSize(webpSize)}");
            Console.WriteLine($"  JPEG 大小: {FormatFileSize(jpegSize)}");

            // 选择更小的格式
            if (webpSize <= jpegSize) {
                Console.WriteLine($"  选择格式: WebP (更小)");
                File.Copy(tempWebp, outputPath, true);
            } else {
                Console.WriteLine($"  选择格式: JPEG (更小)");
                string jpegPath = Path.ChangeExtension(outputPath, ".jpg");
                File.Copy(tempJpeg, jpegPath, true);
                File.Move(jpegPath, outputPath);
            }

        } finally {
            // 清理临时文件
            if (File.Exists(tempWebp)) File.Delete(tempWebp);
            if (File.Exists(tempJpeg)) File.Delete(tempJpeg);
        }
    }
}

// 检测是否有透明度
static bool HasTransparency(Image image) {
    // 检查像素格式是否支持透明度
    return image.PixelType.BitsPerPixel == 32 ||
           image.PixelType.ToString().Contains("Rgba");
}

// 检测是否为简单图形
static bool IsSimpleGraphic(Image image) {
    // 简单启发式：小尺寸或特定宽高比可能是图标/图形
    return image.Width <= 512 && image.Height <= 512;
}

// 检测GIF类型和特征
static (GifType type, int frameCount, bool hasTransparency) AnalyzeGif(string filePath) {
    using var image = Image.Load(filePath);

    // 检查帧数
    int frameCount = image.Frames.Count;
    GifType type = frameCount > 1 ? GifType.Animated : GifType.Static;

    // 检查透明度
    bool hasTransparency = HasTransparency(image);

    return (type, frameCount, hasTransparency);
}

// 获取GIF压缩配置
static GifCompressionConfig GetGifCompressionConfig(string mode, GifType gifType, int originalWidth, int originalHeight) {
    return mode switch {
        "1" => gifType == GifType.Animated
            ? new GifCompressionConfig(800, 600, 64, true, "动画GIF - 博客配图模式")
            : new GifCompressionConfig(1200, 800, 128, false, "静态GIF - 博客配图模式"),
        "2" => new GifCompressionConfig(400, 300, 32, true, "GIF缩略图模式"),
        "3" => gifType == GifType.Animated
            ? new GifCompressionConfig(1200, 900, 128, true, "动画GIF - 头图模式")
            : new GifCompressionConfig(1920, 1080, 256, false, "静态GIF - 头图模式"),
        "4" => new GifCompressionConfig(originalWidth, originalHeight, 64, true, "保持原尺寸 - GIF优化"),
        "5" => GetCustomGifConfig(),
        _ => new GifCompressionConfig(800, 600, 64, true, "默认GIF模式")
    };
}

// 获取自定义GIF配置
static GifCompressionConfig GetCustomGifConfig() {
    Console.Write("请输入目标宽度: ");
    int width = int.TryParse(Console.ReadLine(), out int w) ? w : 800;

    Console.Write("请输入目标高度: ");
    int height = int.TryParse(Console.ReadLine(), out int h) ? h : 600;

    Console.Write("请输入最大颜色数 (8-256): ");
    int maxColors = int.TryParse(Console.ReadLine(), out int c) ? Math.Clamp(c, 8, 256) : 64;

    Console.Write("是否启用抖动? (y/n): ");
    bool enableDithering = Console.ReadLine()?.ToLower() == "y";

    return new GifCompressionConfig(width, height, maxColors, enableDithering, "自定义GIF模式");
}

// 获取默认输出路径
static string GetDefaultOutputPath(string inputPath) {
    string directory = Path.GetDirectoryName(inputPath) ?? "";
    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(inputPath);
    return Path.Combine(directory, $"{fileNameWithoutExt}_compressed.webp");
}

// 获取GIF专用的默认输出路径
static string GetDefaultGifOutputPath(string inputPath) {
    string directory = Path.GetDirectoryName(inputPath) ?? "";
    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(inputPath);
    return Path.Combine(directory, $"{fileNameWithoutExt}_compressed.gif");
}

// 专门的GIF压缩方法
static async Task CompressGifImage(string inputPath, string outputPath, GifCompressionConfig config) {
    using var image = await Image.LoadAsync(inputPath);

    Console.WriteLine($"\n=== GIF压缩处理 ===");
    Console.WriteLine($"配置: {config.Description}");
    Console.WriteLine($"目标尺寸: {config.Width} × {config.Height}");
    Console.WriteLine($"最大颜色数: {config.MaxColors}");
    Console.WriteLine($"抖动: {(config.EnableDithering ? "启用" : "禁用")}");

    // 计算缩放比例，保持宽高比
    double scaleX = (double)config.Width / image.Width;
    double scaleY = (double)config.Height / image.Height;
    double scale = Math.Min(scaleX, scaleY);

    // 如果图片已经比目标尺寸小，则不放大
    if (scale > 1.0) {
        scale = 1.0;
    }

    int newWidth = (int)(image.Width * scale);
    int newHeight = (int)(image.Height * scale);

    // 应用变换
    if (scale < 1.0) {
        image.Mutate(x => x.Resize(newWidth, newHeight));
        Console.WriteLine($"调整尺寸: {image.Width}×{image.Height} -> {newWidth}×{newHeight}");
    }

    // 配置GIF编码器
    var gifEncoder = new GifEncoder();

    // 设置颜色量化器
    IQuantizer quantizer = config.MaxColors switch {
        <= 16 => new WebSafePaletteQuantizer(new QuantizerOptions {
            MaxColors = config.MaxColors,
            Dither = config.EnableDithering ? KnownDitherings.FloydSteinberg : null
        }),
        <= 64 => new OctreeQuantizer(new QuantizerOptions {
            MaxColors = config.MaxColors,
            Dither = config.EnableDithering ? KnownDitherings.FloydSteinberg : null
        }),
        _ => new WuQuantizer(new QuantizerOptions {
            MaxColors = config.MaxColors,
            Dither = config.EnableDithering ? KnownDitherings.FloydSteinberg : null
        })
    };

    // 应用量化
    image.Mutate(x => x.Quantize(quantizer));

    // 保存为GIF格式，保持原有的动画特性
    string gifOutputPath = Path.ChangeExtension(outputPath, ".gif");
    await image.SaveAsGifAsync(gifOutputPath, gifEncoder);

    // 如果输出路径不是.gif，则重命名
    if (outputPath != gifOutputPath && File.Exists(gifOutputPath)) {
        File.Move(gifOutputPath, outputPath);
    }

    Console.WriteLine($"GIF压缩完成: {gifOutputPath}");
}

// 压缩配置结构
public record CompressionConfig(int Width, int Height, int Quality, string Description);

// GIF专用压缩配置
public record GifCompressionConfig(
    int Width,
    int Height,
    int MaxColors,      // 最大颜色数 (8-256)
    bool EnableDithering, // 是否启用抖动
    string Description
);

// GIF类型枚举
public enum GifType {
    Static,    // 静态GIF
    Animated   // 动画GIF
}