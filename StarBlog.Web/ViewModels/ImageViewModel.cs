using System.ComponentModel.DataAnnotations;

namespace StarBlog.Web.ViewModels;

public class ImageViewModel {
    private readonly string _imageUrl;
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Src {
        get => UseRandomImage
            ? $"https://blog.sblt.deali.cn:9000/Api/PicLib/Random/{Id}/{Width}/{Height}"
            : _imageUrl;
        init => _imageUrl = value;
    }

    public string Alt { get; set; }

    [Required] public int Width { get; set; }

    [Required] public int Height { get; set; }

    public bool UseRandomImage => string.IsNullOrWhiteSpace(_imageUrl);
}