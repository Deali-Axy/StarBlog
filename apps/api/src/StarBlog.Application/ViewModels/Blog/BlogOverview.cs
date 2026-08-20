namespace StarBlog.Application.ViewModels.Blog;

/// <summary>
/// 博客信息概况
/// </summary>
public class BlogOverview {
    /// <summary>
    /// 文章数量
    /// </summary>
    public long PostsCount { get; set; }

    /// <summary>
    /// 分类数量
    /// </summary>
    public long CategoriesCount { get; set; }

    /// <summary>
    /// 照片数量
    /// </summary>
    public long PhotosCount { get; set; }

    /// <summary>
    /// 推荐文章数量
    /// </summary>
    public long FeaturedPostsCount { get; set; }

    /// <summary>
    /// 推荐分类数量
    /// </summary>
    public long FeaturedCategoriesCount { get; set; }

    /// <summary>
    /// 推荐照片数量
    /// </summary>
    public long FeaturedPhotosCount { get; set; }
}