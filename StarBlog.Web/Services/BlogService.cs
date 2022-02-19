using FreeSql;
using StarBlog.Data.Models;

namespace StarBlog.Web.Services;

public class BlogService {
    private readonly IBaseRepository<TopPost> _topPostRepo;
    private readonly IBaseRepository<FeaturedPost> _featuredPostRepo;

    public BlogService(IBaseRepository<TopPost> topPostRepo, IBaseRepository<FeaturedPost> featuredPostRepo) {
        _topPostRepo = topPostRepo;
        _featuredPostRepo = featuredPostRepo;
    }

    public Post? GetTopOnePost() {
        return _topPostRepo.Select.Include(a => a.Post.Category).First()?.Post;
    }

    /// <summary>
    /// 获取推荐博客row，一行最多两个博客
    /// </summary>
    /// <returns></returns>
    public List<List<Post>> GetFeaturedPosts() {
        var data = new List<List<Post>>();
        
        var posts= _featuredPostRepo.Select.Include(a => a.Post.Category)
            .ToList(a => a.Post);
        for (var i = 0; i < posts.Count; i += 2) {
            data.Add(new List<Post>{posts[i], posts[i + 1]});
        }

        return data;
    }
}