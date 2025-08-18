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

        foreach (var post in posts) {
            var blogImageDir = Path.Combine(wwwroot, "media", "blog", post.Id);
            if (!Directory.Exists(blogImageDir)) {
                continue;
            }

            logger.LogInformation("处理文章 {PostId} 的图片, {blogImageDir}", post.Id, blogImageDir);

            var files = Directory.GetFiles(blogImageDir);
            var hasChanges = false;
            var fileNameMappings = new Dictionary<string, string>();

            foreach (var file in files) {
                if (!IsImage(file)) {
                    continue;
                }

                logger.LogInformation("处理图片 {FileName}", file);

                try {
                    var result = await CompressImage(file);
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
                } catch (Exception ex) {
                    logger.LogError(ex, "压缩图片失败: {FileName}", file);
                }
            }

            // 如果有文件名变化，需要更新博客内容
            if (hasChanges && fileNameMappings.Count > 0) {
                var updatedContent = UpdateMarkdownImageLinks(post, fileNameMappings);
                if (updatedContent != post.Content) {
                    post.Content = updatedContent;
                    await postRepo.UpdateAsync(post);
                    logger.LogInformation("已更新文章 {PostId} 的图片链接", post.Id);
                }
            }
        }

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
    /// <param name="imagePath">图片路径</param>
    /// <returns>压缩结果</returns>
    private async Task<CompressionResult> CompressImage(string imagePath) {
        var originalInfo = new FileInfo(imagePath);
        var originalSize = originalInfo.Length;

        // 生成输出文件名
        var directory = Path.GetDirectoryName(imagePath)!;
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(imagePath);
        var extension = Path.GetExtension(imagePath).ToLower();

        // 根据原始格式决定输出格式和文件名
        string outputPath;
        if (extension == ".gif") {
            // GIF保持原格式，只优化
            outputPath = Path.Combine(directory, $"{fileNameWithoutExt}_optimized.gif");
        } else {
            // 其他格式转换为WebP
            outputPath = Path.Combine(directory, $"{fileNameWithoutExt}.webp");
        }

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

        // 根据格式保存
        if (extension == ".gif") {
            // GIF格式保持原样，只调整尺寸
            await image.SaveAsGifAsync(outputPath);
        } else {
            // 其他格式转换为WebP
            var webpEncoder = new WebpEncoder {
                Quality = 85,
                Method = WebpEncodingMethod.BestQuality
            };
            await image.SaveAsync(outputPath, webpEncoder);
        }

        var compressedInfo = new FileInfo(outputPath);
        var compressedSize = compressedInfo.Length;
        var compressionRatio = 1.0 - (double)compressedSize / originalSize;

        // 如果压缩后文件更大，保留原文件
        if (compressedSize >= originalSize) {
            File.Delete(outputPath);
            return new CompressionResult {
                Success = false,
                OriginalFilePath = imagePath,
                NewFilePath = imagePath,
                OriginalSize = originalSize,
                CompressedSize = originalSize,
                CompressionRatio = 0
            };
        }

        // 删除原文件，重命名压缩后的文件
        File.Delete(imagePath);
        var finalPath = imagePath;

        // 如果格式发生变化，需要更新文件扩展名
        if (Path.GetExtension(outputPath) != Path.GetExtension(imagePath)) {
            finalPath = Path.ChangeExtension(imagePath, Path.GetExtension(outputPath));
        }

        File.Move(outputPath, finalPath);

        return new CompressionResult {
            Success = true,
            OriginalFilePath = imagePath,
            NewFilePath = finalPath,
            OriginalSize = originalSize,
            CompressedSize = compressedSize,
            CompressionRatio = compressionRatio
        };
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
                    } else {
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