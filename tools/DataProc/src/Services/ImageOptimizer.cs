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

namespace DataProc.Services;

public class ImageOptimizer(
    ILogger<ImageOptimizer> logger,
    IConfiguration conf,
    IBaseRepository<Post> postRepo
) : IService {
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

            foreach (var file in files) {
                if (!IsImage(file)) {
                    continue;
                }

                logger.LogInformation("处理图片 {FileName}", file);

                try {
                    var result = await CompressImage(file, outputDir);
                    if (result.Success) {
                        hasChanges = true;
                        var originalFileName = Path.GetFileName(file);
                        var newFileName = Path.GetFileName(result.NewFilePath);

                        if (originalFileName != newFileName) {
                            fileNameMappings[originalFileName] = newFileName;
                        }

                        logger.LogInformation("图片压缩成功: {OriginalFile} -> {NewFile}, 压缩率: {CompressionRatio:P2}",
                            originalFileName, newFileName, result.CompressionRatio);
                    }
                }
                catch (Exception ex) {
                    logger.LogError(ex, "压缩图片失败: {FileName}", file);
                }
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

        logger.LogInformation("图片压缩完成！");
        logger.LogInformation("压缩后的图片已保存到: {OutputBaseDir}", outputBaseDir);
        logger.LogInformation("您可以检查压缩效果后，手动替换原目录中的图片文件");
        logger.LogInformation("原目录: {OriginalDir}", Path.Combine(wwwroot, "media", "blog"));
        logger.LogInformation("新目录: {NewDir}", outputBaseDir);

        return Result.Ok();
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