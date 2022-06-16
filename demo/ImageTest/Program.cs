// See https://aka.ms/new-console-template for more information

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

Console.WriteLine("Hello, World!");

var imageList = new List<string> {
    "1xdqasb8hblhoo57xthdxu1v6.jpg", "2cs5jqz3o4orwvpr5i5oroyck.jpg", "2hngz6wxz7vzd8ncr6ym1rt7p.jpg", "2kjvh5zl9y0g9ovrqmkodaurt.jpg",
    "wallhaven-753155.jpg",
};

void GetImage(string imagePath, int width, int height) {
    using var image = Image.Load(imagePath);
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
    image.SaveAsPng(@"images\output\output.png");
}

var inputImage = Path.Combine("images", imageList[3]);
GetImage(inputImage, 200, 200);