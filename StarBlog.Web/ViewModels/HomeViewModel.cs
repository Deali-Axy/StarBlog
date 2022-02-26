using StarBlog.Data.Models;

namespace StarBlog.Web.ViewModels;

public class HomeViewModel {
    /// <summary>
    /// 置顶博客
    /// </summary>
    public Post? TopPost { get; set; }

    /// <summary>
    /// 推荐博客
    /// </summary>
    public List<Post> FeaturedPosts { get; set; } = new();
    
    /// <summary>
    /// 推荐照片，原则上只能三个
    /// </summary>
    public List<Photo> FeaturedPhotos { get; set; } = new();

    /// <summary>
    /// 推荐分类，原则上三个
    /// </summary>
    public List<FeaturedCategory> FeaturedCategories { get; set; } = new();
}