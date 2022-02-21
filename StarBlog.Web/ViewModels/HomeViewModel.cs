using StarBlog.Data.Models;

namespace StarBlog.Web.ViewModels;

public class HomeViewModel {
    /// <summary>
    /// 置顶博客
    /// </summary>
    public Post? TopPost { get; set; }

    /// <summary>
    /// 推荐博客行，一行最多两个博客
    /// </summary>
    public List<List<Post>> FeaturedPosts { get; set; } = new();
}