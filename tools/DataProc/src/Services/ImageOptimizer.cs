using FluentResults;
using FreeSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StarBlog.Data.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Collections.Concurrent;

namespace DataProc.Services;

public class ProcessingStats {
    public string PostId { get; set; } = "";
    public int TotalImages { get; set; }
    public int ProcessedImages { get; set; }
    public int SuccessfulCompressions { get; set; }
    public int FailedCompressions { get; set; }
    public long OriginalSize { get; set; }
    public long CompressedSize { get; set; }
    public double CompressionRatio => OriginalSize > 0 ? 1.0 - (double)CompressedSize / OriginalSize : 0;
    public long SavedBytes => OriginalSize - CompressedSize;
}

public class ImageOptimizer : IService {
    private readonly ILogger<ImageOptimizer> logger;
    private readonly IConfiguration conf;
    private readonly IBaseRepository<Post> postRepo;

    // 统计信息 - 使用线程安全的集合
    private readonly ConcurrentBag<ProcessingStats> _processingStats = new();
    private int _totalImages = 0;
    private int _processedImages = 0;
    private int _successfulCompressions = 0;
    private int _failedCompressions = 0;
    private long _totalOriginalSize = 0;
    private long _totalCompressedSize = 0;
    private readonly ConcurrentDictionary<string, int> _formatStats = new();

    // 并发控制
    private readonly SemaphoreSlim _concurrencyLimiter;
    private readonly int _maxConcurrency;

    public ImageOptimizer(
        ILogger<ImageOptimizer> logger,
        IConfiguration conf,
        IBaseRepository<Post> postRepo) {
        this.logger = logger;
        this.conf = conf;
        this.postRepo = postRepo;

        // 设置最大并发数，默认为CPU核心数
        _maxConcurrency = conf.GetValue<int>("ImageOptimizer:MaxConcurrency", Environment.ProcessorCount);
        _concurrencyLimiter = new SemaphoreSlim(_maxConcurrency, _maxConcurrency);

        logger.LogInformation("图片压缩器初始化 - 最大并发数: {MaxConcurrency}", _maxConcurrency);
    }
    public async Task<Result> Run() {
        var posts = await postRepo.Select.ToListAsync();
        var wwwroot = conf.GetValue<string>("StarBlog:wwwroot");
        if (string.IsNullOrWhiteSpace(wwwroot)) {
            throw new Exception("wwwroot 配置错误");
        }

        // 获取输出目录配置
        var outputBaseDir = conf.GetValue<string>("ImageOptimizer:OutputDir");
        if (string.IsNullOrWhiteSpace(outputBaseDir)) {
            throw new Exception("未配置输出目录");
        }

        logger.LogInformation("压缩后图片将保存到: {OutputBaseDir}", outputBaseDir);

        // 确保输出基础目录存在
        Directory.CreateDirectory(outputBaseDir);

        logger.LogInformation("开始处理 {TotalPosts} 篇文章的图片", posts.Count);
        var processedCount = 0;

        foreach (var post in posts) {
            processedCount++;
            logger.LogInformation("处理进度: {Current}/{Total} - 文章 {PostId}",
                processedCount, posts.Count, post.Id);

            var blogImageDir = Path.Combine(wwwroot, "media", "blog", post.Id);
            if (!Directory.Exists(blogImageDir)) {
                continue;
            }

            // 创建对应的输出目录
            var outputDir = Path.Combine(outputBaseDir, post.Id);
            Directory.CreateDirectory(outputDir);

            logger.LogInformation("处理文章 {PostId} 的图片, 源目录: {SourceDir}, 输出目录: {OutputDir}",
                post.Id, blogImageDir, outputDir);

            var files = Directory.GetFiles(blogImageDir);
            var hasChanges = false;
            var fileNameMappings = new Dictionary<string, string>();

            // 初始化当前文章的统计信息
            var postStats = new ProcessingStats { PostId = post.Id };
            var imageFiles = files.Where(IsImage).ToArray();
            postStats.TotalImages = imageFiles.Length;
            _totalImages += imageFiles.Length;

            // 并行处理图片
            var compressionTasks = imageFiles.Select(async file => {
                await _concurrencyLimiter.WaitAsync();
                try {
                    logger.LogInformation("处理图片 {FileName}", file);
                    Interlocked.Increment(ref _processedImages);

                    var result = await CompressImage(file, outputDir);

                    // 线程安全地更新统计信息
                    Interlocked.Add(ref _totalOriginalSize, result.OriginalSize);
                    Interlocked.Add(ref _totalCompressedSize, result.CompressedSize);

                    if (result.Success) {
                        hasChanges = true;
                        Interlocked.Increment(ref _successfulCompressions);

                        var originalFileName = Path.GetFileName(file);
                        var newFileName = Path.GetFileName(result.NewFilePath);

                        if (originalFileName != newFileName) {
                            lock (fileNameMappings) {
                                fileNameMappings[originalFileName] = newFileName;
                            }
                        }

                        // 线程安全地统计格式信息
                        var outputFormat = Path.GetExtension(result.NewFilePath).ToLower();
                        _formatStats.AddOrUpdate(outputFormat, 1, (key, value) => value + 1);

                        logger.LogInformation("图片压缩成功: {OriginalFile} -> {NewFile}, 压缩率: {CompressionRatio:P2}",
                            originalFileName, newFileName, result.CompressionRatio);
                    } else {
                        Interlocked.Increment(ref _failedCompressions);
                    }

                    return result;
                }
                catch (Exception ex) {
                    logger.LogError(ex, "压缩图片失败: {FileName}", file);
                    Interlocked.Increment(ref _failedCompressions);
                    return null;
                }
                finally {
                    _concurrencyLimiter.Release();
                }
            });

            // 等待所有图片处理完成
            var results = await Task.WhenAll(compressionTasks);

            // 收集文章级别的统计信息
            foreach (var result in results.Where(r => r != null)) {
                postStats.OriginalSize += result.OriginalSize;
                postStats.CompressedSize += result.CompressedSize;
                postStats.ProcessedImages++;

                if (result.Success) {
                    postStats.SuccessfulCompressions++;
                } else {
                    postStats.FailedCompressions++;
                }
            }

            // 保存文章统计信息
            if (postStats.TotalImages > 0) {
                _processingStats.Add(postStats);
            }

            // 如果有文件名变化，需要更新博客内容
            if (hasChanges && fileNameMappings.Count > 0) {
                try {
                    var updatedContent = UpdateMarkdownImageLinks(post, fileNameMappings);
                    if (updatedContent != post.Content) {
                        post.Content = updatedContent;
                        await postRepo.UpdateAsync(post);
                        logger.LogInformation("已更新文章 {PostId} 的图片链接", post.Id);
                    }
                }
                catch (Exception ex) {
                    logger.LogError(ex, "更新文章 {PostId} 的图片链接失败", post.Id);
                    // 数据库更新失败不影响继续处理其他文章
                }
            }
        }

        // 生成汇总报告
        GenerateSummaryReport(outputBaseDir, wwwroot);

        return Result.Ok();
    }

    /// <summary>
    /// 生成汇总报告
    /// </summary>
    private void GenerateSummaryReport(string outputBaseDir, string wwwroot) {
        logger.LogInformation("");
        logger.LogInformation("🎉 ================ 图片压缩汇总报告 ================");
        logger.LogInformation("");

        // 基本统计
        logger.LogInformation("📊 基本统计:");
        logger.LogInformation("   • 处理的文章数量: {ProcessedPosts}", _processingStats.Count);
        logger.LogInformation("   • 发现的图片总数: {TotalImages}", _totalImages);
        logger.LogInformation("   • 处理的图片数量: {ProcessedImages}", _processedImages);
        logger.LogInformation("   • 成功压缩数量: {SuccessfulCompressions}", _successfulCompressions);
        logger.LogInformation("   • 压缩失败数量: {FailedCompressions}", _failedCompressions);
        logger.LogInformation("");

        // 文件大小统计
        var totalSavedBytes = _totalOriginalSize - _totalCompressedSize;
        var overallCompressionRatio = _totalOriginalSize > 0 ? 1.0 - (double)_totalCompressedSize / _totalOriginalSize : 0;

        logger.LogInformation("💾 文件大小统计:");
        logger.LogInformation("   • 原始总大小: {OriginalSize}", FormatFileSize(_totalOriginalSize));
        logger.LogInformation("   • 压缩后总大小: {CompressedSize}", FormatFileSize(_totalCompressedSize));
        logger.LogInformation("   • 节省空间: {SavedSize}", FormatFileSize(totalSavedBytes));
        logger.LogInformation("   • 总体压缩率: {CompressionRatio:P2}", overallCompressionRatio);
        logger.LogInformation("");

        // 格式统计
        if (_formatStats.Count > 0) {
            logger.LogInformation("📁 输出格式统计:");
            foreach (var format in _formatStats.OrderByDescending(x => x.Value)) {
                logger.LogInformation("   • {Format}: {Count} 个文件", format.Key.ToUpper(), format.Value);
            }
            logger.LogInformation("");
        }

        // 前10个压缩效果最好的文章
        var topCompressionPosts = _processingStats
            .Where(p => p.SuccessfulCompressions > 0)
            .OrderByDescending(p => p.SavedBytes)
            .Take(10)
            .ToList();

        if (topCompressionPosts.Count > 0) {
            logger.LogInformation("🏆 压缩效果最佳的文章 (前10名):");
            for (int i = 0; i < topCompressionPosts.Count; i++) {
                var post = topCompressionPosts[i];
                logger.LogInformation("   {Rank}. 文章 {PostId}: 节省 {SavedSize}, 压缩率 {CompressionRatio:P2} ({SuccessfulCount}/{TotalCount} 张图片)",
                    i + 1, post.PostId, FormatFileSize(post.SavedBytes), post.CompressionRatio,
                    post.SuccessfulCompressions, post.TotalImages);
            }
            logger.LogInformation("");
        }

        // 失败统计
        var failedPosts = _processingStats.Where(p => p.FailedCompressions > 0).ToList();
        if (failedPosts.Count > 0) {
            logger.LogInformation("⚠️  压缩失败统计:");
            foreach (var post in failedPosts.OrderByDescending(p => p.FailedCompressions)) {
                logger.LogInformation("   • 文章 {PostId}: {FailedCount} 张图片压缩失败",
                    post.PostId, post.FailedCompressions);
            }
            logger.LogInformation("");
        }

        // 目录信息
        logger.LogInformation("📂 目录信息:");
        logger.LogInformation("   • 原始图片目录: {OriginalDir}", Path.Combine(wwwroot, "media", "blog"));
        logger.LogInformation("   • 压缩后图片目录: {OutputDir}", outputBaseDir);
        logger.LogInformation("");

        // 操作建议
        logger.LogInformation("💡 下一步操作建议:");
        logger.LogInformation("   1. 检查压缩后的图片质量和效果");
        logger.LogInformation("   2. 确认无误后，可以手动替换原目录中的图片文件");
        logger.LogInformation("   3. 建议先备份原始图片目录");
        if (failedPosts.Count > 0) {
            logger.LogInformation("   4. 检查压缩失败的图片，可能需要手动处理");
        }
        logger.LogInformation("");
        logger.LogInformation("🎉 ================ 报告结束 ================");
        logger.LogInformation("");
    }

    /// <summary>
    /// 格式化文件大小显示
    /// </summary>
    private static string FormatFileSize(long bytes) {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
    }

    bool IsImage(string fileName) {
        var ext = Path.GetExtension(fileName);
        return ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".gif", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".webp", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 压缩单个图片
    /// </summary>
    /// <param name="imagePath">原图片路径</param>
    /// <param name="outputDirectory">输出目录</param>
    /// <returns>压缩结果</returns>
    private async Task<CompressionResult> CompressImage(string imagePath, string outputDirectory) {
        var originalInfo = new FileInfo(imagePath);
        var originalSize = originalInfo.Length;

        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(imagePath);
        var extension = Path.GetExtension(imagePath).ToLower();

        using var image = await Image.LoadAsync(imagePath);

        // 计算缩放比例，保持宽高比，最大宽度1200px
        const int maxWidth = 1200;
        double scale = 1.0;
        if (image.Width > maxWidth) {
            scale = (double)maxWidth / image.Width;
        }

        if (scale < 1.0) {
            var newWidth = (int)(image.Width * scale);
            var newHeight = (int)(image.Height * scale);
            image.Mutate(x => x.Resize(newWidth, newHeight));
        }

        // 智能选择输出格式
        var (outputFormat, outputPath) =
            await SelectOptimalFormat(image, imagePath, outputDirectory, fileNameWithoutExt, extension);

        var compressedInfo = new FileInfo(outputPath);
        var compressedSize = compressedInfo.Length;
        var compressionRatio = 1.0 - (double)compressedSize / originalSize;

        // 如果压缩后文件更大，删除压缩文件，复制原文件到输出目录
        if (compressedSize >= originalSize) {
            File.Delete(outputPath);

            // 复制原文件到输出目录
            var originalFileName = Path.GetFileName(imagePath);
            var finalPath = Path.Combine(outputDirectory, originalFileName);
            File.Copy(imagePath, finalPath, true);

            return new CompressionResult {
                Success = false,
                OriginalFilePath = imagePath,
                NewFilePath = finalPath,
                OriginalSize = originalSize,
                CompressedSize = originalSize,
                CompressionRatio = 0
            };
        }

        logger.LogDebug("选择的输出格式: {OutputFormat}", outputFormat);

        return new CompressionResult {
            Success = true,
            OriginalFilePath = imagePath,
            NewFilePath = outputPath,
            OriginalSize = originalSize,
            CompressedSize = compressedSize,
            CompressionRatio = compressionRatio
        };
    }

    /// <summary>
    /// 智能选择最优的输出格式
    /// </summary>
    /// <param name="image">图片对象</param>
    /// <param name="originalPath">原始文件路径</param>
    /// <param name="outputDirectory">输出目录</param>
    /// <param name="fileNameWithoutExt">不含扩展名的文件名</param>
    /// <param name="originalExtension">原始扩展名</param>
    /// <returns>输出格式和路径</returns>
    private async Task<(string format, string outputPath)> SelectOptimalFormat(
        Image image, string originalPath, string outputDirectory, string fileNameWithoutExt, string originalExtension) {
        // GIF格式特殊处理，保持原格式
        if (originalExtension == ".gif") {
            var gifPath = Path.Combine(outputDirectory, $"{fileNameWithoutExt}.gif");
            await image.SaveAsGifAsync(gifPath);
            return ("GIF", gifPath);
        }

        // 分析图片特征
        bool hasTransparency = HasTransparency(image);
        bool isSimpleGraphic = IsSimpleGraphic(image);

        logger.LogDebug("图片分析 - 透明度: {HasTransparency}, 图片类型: {ImageType}",
            hasTransparency, isSimpleGraphic ? "图形/图标" : "照片/复杂图像");

        // 智能选择格式
        if (hasTransparency) {
            // 有透明度，使用 WebP
            var webpPath = Path.Combine(outputDirectory, $"{fileNameWithoutExt}.webp");
            var webpEncoder = new WebpEncoder {
                Quality = 85,
                Method = WebpEncodingMethod.BestQuality
            };
            await image.SaveAsync(webpPath, webpEncoder);
            return ("WebP (保持透明度)", webpPath);
        }
        else if (isSimpleGraphic) {
            // 简单图形，使用 WebP
            var webpPath = Path.Combine(outputDirectory, $"{fileNameWithoutExt}.webp");
            var webpEncoder = new WebpEncoder {
                Quality = 85,
                Method = WebpEncodingMethod.BestQuality
            };
            await image.SaveAsync(webpPath, webpEncoder);
            return ("WebP (图形优化)", webpPath);
        }
        else {
            // 复杂图像/照片，比较 WebP 和 JPEG
            return await CompareWebpAndJpeg(image, outputDirectory, fileNameWithoutExt);
        }
    }

    /// <summary>
    /// 比较WebP和JPEG格式，选择文件更小的格式
    /// </summary>
    private async Task<(string format, string outputPath)> CompareWebpAndJpeg(
        Image image, string outputDirectory, string fileNameWithoutExt) {
        // 创建临时文件测试压缩效果
        string tempWebp = Path.GetTempFileName() + ".webp";
        string tempJpeg = Path.GetTempFileName() + ".jpg";

        try {
            // 测试 WebP
            var webpEncoder = new WebpEncoder {
                Quality = 85,
                Method = WebpEncodingMethod.BestQuality
            };
            await image.SaveAsync(tempWebp, webpEncoder);

            // 测试 JPEG
            var jpegEncoder = new JpegEncoder {
                Quality = 85
            };
            await image.SaveAsync(tempJpeg, jpegEncoder);

            // 比较文件大小
            var webpSize = new FileInfo(tempWebp).Length;
            var jpegSize = new FileInfo(tempJpeg).Length;

            logger.LogDebug("格式比较 - WebP: {WebpSize} bytes, JPEG: {JpegSize} bytes", webpSize, jpegSize);

            // 选择更小的格式
            if (webpSize <= jpegSize) {
                var webpPath = Path.Combine(outputDirectory, $"{fileNameWithoutExt}.webp");
                File.Copy(tempWebp, webpPath, true);
                return ("WebP (更小)", webpPath);
            }
            else {
                var jpegPath = Path.Combine(outputDirectory, $"{fileNameWithoutExt}.jpg");
                File.Copy(tempJpeg, jpegPath, true);
                return ("JPEG (更小)", jpegPath);
            }
        }
        finally {
            // 清理临时文件
            if (File.Exists(tempWebp)) File.Delete(tempWebp);
            if (File.Exists(tempJpeg)) File.Delete(tempJpeg);
        }
    }

    /// <summary>
    /// 检测图片是否有透明度
    /// </summary>
    private static bool HasTransparency(Image image) {
        // 检查像素格式是否支持透明度
        return image.PixelType.BitsPerPixel == 32 ||
               image.PixelType.ToString().Contains("Rgba");
    }

    /// <summary>
    /// 检测是否为简单图形
    /// </summary>
    private static bool IsSimpleGraphic(Image image) {
        // 简单启发式：小尺寸可能是图标/图形
        return image.Width <= 512 && image.Height <= 512;
    }

    /// <summary>
    /// 更新Markdown内容中的图片链接
    /// </summary>
    /// <param name="post">博客文章</param>
    /// <param name="fileNameMappings">文件名映射关系</param>
    /// <returns>更新后的内容</returns>
    private string UpdateMarkdownImageLinks(Post post, Dictionary<string, string> fileNameMappings) {
        if (post.Content == null) {
            return string.Empty;
        }

        var document = Markdown.Parse(post.Content);

        foreach (var node in document.AsEnumerable()) {
            if (node is not ParagraphBlock { Inline: { } } paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline) {
                if (inline is not LinkInline { IsImage: true } linkInline) continue;
                if (string.IsNullOrWhiteSpace(linkInline.Url)) continue;

                var imgUrl = Uri.UnescapeDataString(linkInline.Url);
                if (imgUrl.StartsWith("http")) continue; // 跳过外部链接

                // 获取图片文件名
                var imgFileName = Path.GetFileName(imgUrl);

                // 检查是否需要替换文件名
                if (fileNameMappings.TryGetValue(imgFileName, out var newFileName)) {
                    // 替换文件名，保持路径结构
                    var directory = Path.GetDirectoryName(imgUrl);
                    if (string.IsNullOrEmpty(directory)) {
                        linkInline.Url = newFileName;
                    }
                    else {
                        linkInline.Url = Path.Combine(directory, newFileName).Replace('\\', '/');
                    }

                    logger.LogDebug("更新图片链接: {OldUrl} -> {NewUrl}", imgUrl, linkInline.Url);
                }
            }
        }

        using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }
}

/// <summary>
/// 图片压缩结果
/// </summary>
public class CompressionResult {
    public bool Success { get; set; }
    public string OriginalFilePath { get; set; } = string.Empty;
    public string NewFilePath { get; set; } = string.Empty;
    public long OriginalSize { get; set; }
    public long CompressedSize { get; set; }
    public double CompressionRatio { get; set; }
}