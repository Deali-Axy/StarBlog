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
    [Obsolete("不需要分出来row了，直接使用 GetFeaturedPosts() 即可")]
    public List<List<Post>> GetFeaturedPostRows() {
        var data = new List<List<Post>>();

        var posts = GetFeaturedPosts();
        for (var i = 0; i < posts.Count; i += 2) {
            data.Add(new List<Post> {posts[i], posts[i + 1]});
        }

        return data;
    }

    public List<Post> GetFeaturedPosts() {
        return _featuredPostRepo.Select.Include(a => a.Post.Category)
            .ToList(a => a.Post);
    }

    public FeaturedPost AddFeaturedPost(Post post) {
        var item = _featuredPostRepo.Where(a => a.PostId == post.Id).First();
        if (item != null) return item;
        item = new FeaturedPost {PostId = post.Id};
        _featuredPostRepo.Insert(item);
        return item;
    }

    public int DeleteFeaturedPost(Post post) {
        var item = _featuredPostRepo.Where(a => a.PostId == post.Id).First();
        return item == null ? 0 : _featuredPostRepo.Delete(item);
    }

    /// <summary>
    /// 设置置顶博客
    /// </summary>
    /// <param name="post"></param>
    /// <returns>返回 <see cref="TopPost"/> 对象和删除原有置顶博客的行数</returns>
    public (TopPost, int) SetTopPost(Post post) {
        var rows = _topPostRepo.Select.ToDelete().ExecuteAffrows();
        var item = new TopPost {PostId = post.Id};
        _topPostRepo.Insert(item);
        return (item, rows);
    }
}