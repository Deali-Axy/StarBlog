using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace StarBlog.Web.Services;

/// <summary>
/// 图片库服务
/// </summary>
public class PicLibService {
    private readonly IWebHostEnvironment _environment;
    private readonly Random _random;
    public List<string> ImageList { get; set; } = new();

    public PicLibService(IWebHostEnvironment environment) {
        _environment = environment;
        _random = Random.Shared;

        var importPath = Path.Combine(_environment.WebRootPath, "media", "picture_library");
        var root = new DirectoryInfo(importPath);
        foreach (var file in root.GetFiles()) {
            ImageList.Add(file.FullName);
        }
    }

    /// <summary>
    /// 生成指定尺寸图片
    /// </summary>
    /// <param name="imagePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static async Task<(Image, IImageFormat)> GenerateSizedImageAsync(string imagePath, int width, int height) {
        await using var fileStream = new FileStream(imagePath, FileMode.Open);
        var (image, format) = await Image.LoadWithFormatAsync(fileStream);

        Rectangle cropRect;
        int newWidth;
        int newHeight;

        // 横屏图片
        if (image.Width > image.Height) {
            if (width > image.Width) {
                newWidth = width;
                newHeight = height;
            }
            else {
                newHeight = height;
                newWidth = image.Width / image.Height * newHeight;
            }

            cropRect = new Rectangle((newWidth - width) / 2, 0, width, height);
        }
        // 竖屏图片
        else {
            if (height > image.Height) {
                newWidth = width;
                newHeight = height;
            }
            else {
                newWidth = width;
                newHeight = newWidth * image.Height / image.Width;
            }

            cropRect = new Rectangle(0, (newHeight - height) / 2, width, height);
        }

        image.Mutate(a => a.Resize(newWidth, newHeight));
        image.Mutate(a => a.Crop(cropRect));

        return (image, format);
    }

    /// <summary>
    /// 从图片库中获取随机图片
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="seed"></param>
    /// <returns></returns>
    public async Task<(Image, IImageFormat)> GetRandomImageAsync(int width, int height, string? seed = null) {
        var rnd = seed == null ? _random : new Random(seed.GetHashCode());
        var imagePath = ImageList[rnd.Next(0, ImageList.Count)];
        return await GenerateSizedImageAsync(imagePath, width, height);
    }

    /// <summary>
    /// 从图片库中获取随机图片
    /// </summary>
    /// <param name="seed"></param>
    /// <returns></returns>
    public async Task<(Image, IImageFormat)> GetRandomImageAsync(string? seed = null) {
        var rnd = seed == null ? _random : new Random(seed.GetHashCode());
        var imagePath = ImageList[rnd.Next(0, ImageList.Count)];
        await using var fileStream = new FileStream(imagePath, FileMode.Open);
        return await Image.LoadWithFormatAsync(fileStream);
    }
}