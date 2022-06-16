// See https://aka.ms/new-console-template for more information

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

Console.WriteLine("Hello, World!");

var imgPath1 = @"D:\Code\StarBlog\StarBlog.Web\wwwroot\assets\photography\10.jpg";
var imgPath2 = @"D:\Code\StarBlog\StarBlog.Web\wwwroot\assets\photography\89048309f52ff0d70f51ac3c38a39c80.jpg";


void GetImage(string imagePath, int width, int height) {
    using var image = Image.Load(imagePath);
    int newWidth;
    int newHeight;
    Rectangle cropRect;

    if (image.Width > image.Height) {
        image.Mutate(a => a.Resize(0, height));
        cropRect = new Rectangle((image.Width - width) / 2, 0, width, height);
    }
    else {
        image.Mutate(a => a.Resize(width, 0));
        cropRect = new Rectangle(0, (image.Height - height) / 2, width, height);
    }

    image.Mutate(a => a.Crop(cropRect));
    image.SaveAsPng(@"D:\Code\StarBlog\demo\ImageTest\images\output.png");
}

GetImage(imgPath2, 200, 200);