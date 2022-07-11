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

    [Obsolete("有些图片会破坏比例、或者裁剪失败")]
    public static async Task<(Image, IImageFormat)> GenerateSizedImageAsyncOld(string imagePath, int width, int height) {
        await using var fileStream = new FileStream(imagePath, FileMode.Open);
        var (image, format) = await Image.LoadWithFormatAsync(fileStream);

        var originWidth = image.Width;
        var originHeight = image.Height;

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
                // 按比例放大后转换为int类型会丢失小数部分，所以需要+1
                newWidth = image.Width / image.Height * newHeight + 1;
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
                newHeight = newWidth * image.Height / image.Width + 1;
            }

            cropRect = new Rectangle(0, (newHeight - height) / 2, width, height);
        }

        image.Mutate(a => a.Resize(newWidth, newHeight));

        try {
            image.Mutate(a => a.Crop(cropRect));
        }
        catch (Exception ex) {
            Console.WriteLine($"crop image error: {ex.Message}");
            Console.WriteLine($"image size={originWidth}, {originHeight}; " +
                              $"new size={newWidth},{newHeight}; " +
                              $"crop size={width},{height}");
        }

        return (image, format);
    }

    /// <summary>
    /// 求最大公约数
    /// </summary>
    /// <param name="m"></param>
    /// <param name="n"></param>
    /// <returns>最大公约数</returns>
    private static int GetGreatestCommonDivisor(int m, int n) {
        if (m < n) (n, m) = (m, n);

        while (n != 0) {
            var r = m % n;
            m = n;
            n = r;
        }
        return m;
    }

    /// <summary>
    /// 获取图片比例
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns>宽度 x 高度</returns>
    private static (double, double) GetPhotoScale(int width, int height) {
        if (width == height) return (1, 1);
        var gcd = GetGreatestCommonDivisor(width, height);
        return ((double)width / gcd, (double)height / gcd);
    }

    /// <summary>
    /// 生成指定尺寸图片
    /// </summary>
    /// <param name="imagePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    async Task<(Image, IImageFormat)> GenerateSizedImageAsync(string imagePath, int width, int height) {
        await using var fileStream = new FileStream(imagePath, FileMode.Open);
        var (image, format) = await Image.LoadWithFormatAsync(fileStream);

        // 输出尺寸超出原图片尺寸，放大
        if (width > image.Width && height > image.Height) {
            image.Mutate(a => a.Resize(width, height));
        }
        else if (width > image.Width || height > image.Height) {
            // 改变比例大的边
            if (width / image.Width < height / image.Height)
                image.Mutate(a => a.Resize(0, height));
            else
                image.Mutate(a => a.Resize(width, 0));
        }

        // 将输入的尺寸作为裁剪比例
        var (scaleWidth, scaleHeight) = GetPhotoScale(width, height);
        var cropWidth = image.Width;
        var cropHeight = (int)(image.Width / scaleWidth * scaleHeight);
        if (cropHeight > image.Height) {
            cropHeight = image.Height;
            cropWidth = (int)(image.Height / scaleHeight * scaleWidth);
        }

        var cropRect = new Rectangle((image.Width - cropWidth) / 2, (image.Height - cropHeight) / 2, cropWidth, cropHeight);
        image.Mutate(a => a.Crop(cropRect));
        image.Mutate(a => a.Resize(width, height));

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