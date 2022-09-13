﻿using System.IO.Compression;
using System.Text;
using FreeSql;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels.Blog;

namespace StarBlog.Web.Services;

public class BlogService {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly IBaseRepository<Photo> _photoRepo;
    private readonly IBaseRepository<TopPost> _topPostRepo;
    private readonly IBaseRepository<FeaturedPost> _fPostRepo;
    private readonly IBaseRepository<FeaturedCategory> _fCategoryRepo;
    private readonly IBaseRepository<FeaturedPhoto> _fPhotoRepo;

    public BlogService(IBaseRepository<TopPost> topPostRepo, IBaseRepository<FeaturedPost> fPostRepo, IBaseRepository<Post> postRepo,
        IBaseRepository<Category> categoryRepo, IBaseRepository<Photo> photoRepo, IBaseRepository<FeaturedCategory> fCategoryRepo,
        IBaseRepository<FeaturedPhoto> fPhotoRepo) {
        _topPostRepo = topPostRepo;
        _fPostRepo = fPostRepo;
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _photoRepo = photoRepo;
        _fCategoryRepo = fCategoryRepo;
        _fPhotoRepo = fPhotoRepo;
    }

    /// <summary>
    /// 获取博客信息概况
    /// </summary>
    /// <returns></returns>
    public BlogOverview Overview() {
        return new BlogOverview {
            PostsCount = _postRepo.Select.Count(),
            CategoriesCount = _categoryRepo.Select.Count(),
            PhotosCount = _photoRepo.Select.Count(),
            FeaturedPostsCount = _fPostRepo.Select.Count(),
            FeaturedCategoriesCount = _fCategoryRepo.Select.Count(),
            FeaturedPhotosCount = _fPhotoRepo.Select.Count()
        };
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
        return _fPostRepo.Select.Include(a => a.Post.Category)
            .ToList(a => a.Post);
    }

    public FeaturedPost AddFeaturedPost(Post post) {
        var item = _fPostRepo.Where(a => a.PostId == post.Id).First();
        if (item != null) return item;
        item = new FeaturedPost {PostId = post.Id};
        _fPostRepo.Insert(item);
        return item;
    }

    public int DeleteFeaturedPost(Post post) {
        var item = _fPostRepo.Where(a => a.PostId == post.Id).First();
        return item == null ? 0 : _fPostRepo.Delete(item);
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

    /// <summary>
    /// 获取文章的状态列表
    /// </summary>
    /// <returns></returns>
    public List<string?> GetStatusList() {
        return _postRepo.Select.GroupBy(a => a.Status)
            .ToList(a => a.Key);
    }

    /// <summary>
    /// 上传博客
    /// todo 只完成了解压部分，导入部分待实现
    /// </summary>
    /// <returns></returns>
    public async Task<Post> Upload(PostCreationDto dto, IFormFile file) {
        var tempFile = Path.GetTempFileName();
        await using (var fs = new FileStream(tempFile, FileMode.Create)) {
            await file.CopyToAsync(fs);
        }

        var extractPath = Path.Combine(Path.GetTempPath(), "StarBlog", Guid.NewGuid().ToString());
        // 使用 GBK 编码解压，防止中文文件名乱码
        // 参考：https://www.cnblogs.com/liguix/p/11883248.html
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ZipFile.ExtractToDirectory(tempFile, extractPath, Encoding.GetEncoding("GBK"));

        throw new NotImplementedException();
    }
}