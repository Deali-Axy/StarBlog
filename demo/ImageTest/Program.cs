// See https://aka.ms/new-console-template for more information

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

Console.WriteLine("Hello, World!");

var imageList = new List<string> { "70.jpg" };

void GetImage(string imagePath, int width, int height) {
    using var image = Image.Load(imagePath);
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
    image.SaveAsPng(@"images\output\output.png");
}

var inputImage = Path.Combine("images", imageList[0]);
GetImage(inputImage, 600, 400);