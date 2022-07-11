// See https://aka.ms/new-console-template for more information

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

Console.WriteLine("Hello, World!");

List<string> ScanImages() {
    var data = new List<string>();
    foreach (var file in Directory.GetFiles("images")) {
        Console.WriteLine(file);
        data.Add(file);
    }

    return data;
}

// 求最大公约数
int GetGreatestCommonDivisor(int m, int n) {
    if (m < n) {
        (n, m) = (m, n);
    }

    while (n != 0) {
        var r = m % n;
        m = n;
        n = r;
    }

    return m;
}

(double, double) GetPhotoScale(int width, int height) {
    if (width == height) return (1, 1);
    var gcd = GetGreatestCommonDivisor(width, height);
    return ((double)width / gcd, (double)height / gcd);
}

void GetImage(string imagePath, int width, int height) {
    var image = Image.Load(imagePath);

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
    image.SaveAsJpeg(@"images\output\output.jpg");
}

async Task<(Image, IImageFormat)> GetImage2(string imagePath, int width, int height) {
    await using var fileStream = new FileStream(imagePath, FileMode.Open);
    var (image, format) = await Image.LoadWithFormatAsync(fileStream);

    Console.WriteLine($"origin image={image.Width},{image.Height}");

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

    Console.WriteLine($"Resize={image.Width},{image.Height}");

    // 将输入的尺寸作为裁剪比例
    var (scaleWidth, scaleHeight) = GetPhotoScale(width, height);
    var cropWidth = image.Width;
    var cropHeight = (int)(image.Width / scaleWidth * scaleHeight);
    if (cropHeight > image.Height) {
        cropHeight = image.Height;
        cropWidth = (int)(image.Height / scaleHeight * scaleWidth);
    }

    var cropRect = new Rectangle((image.Width - cropWidth) / 2, (image.Height - cropHeight) / 2, cropWidth, cropHeight);
    Console.WriteLine(cropRect.ToString());
    image.Mutate(a => a.Crop(cropRect));
    image.Mutate(a => a.Resize(width, height));

    return (image, format);
}

async void Run() {
    var (image, _) = await GetImage2(ScanImages()[1], 200, 300);
    await image.SaveAsJpegAsync(@"images\output\output.jpg");
    Console.WriteLine("saved.");
}

Run();

Console.Read();