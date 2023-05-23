using System.IO.Compression;
using System.Text;
using FreeSql;
using StarBlog.Share.Utils;
using StarBlog.Data.Models;
using StarBlog.Share;
using StarBlog.Web.ViewModels.Blog;

namespace StarBlog.Web.Services;

public class BlogService {
    private readonly IWebHostEnvironment _environment;
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly IBaseRepository<Photo> _photoRepo;
    private readonly IBaseRepository<TopPost> _topPostRepo;
    private readonly IBaseRepository<FeaturedPost> _fPostRepo;
    private readonly IBaseRepository<FeaturedCategory> _fCategoryRepo;
    private readonly IBaseRepository<FeaturedPhoto> _fPhotoRepo;

    public BlogService(IBaseRepository<TopPost> topPostRepo, IBaseRepository<FeaturedPost> fPostRepo, IBaseRepository<Post> postRepo,
        IBaseRepository<Category> categoryRepo, IBaseRepository<Photo> photoRepo, IBaseRepository<FeaturedCategory> fCategoryRepo,
        IBaseRepository<FeaturedPhoto> fPhotoRepo, IWebHostEnvironment environment) {
        _topPostRepo = topPostRepo;
        _fPostRepo = fPostRepo;
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _photoRepo = photoRepo;
        _fCategoryRepo = fCategoryRepo;
        _fPhotoRepo = fPhotoRepo;
        _environment = environment;
    }

    /// <summary>
    /// 获取博客信息概况
    /// </summary>
    /// <returns></returns>
    public async Task<BlogOverview> Overview() {
        return new BlogOverview {
            PostsCount = await _postRepo.Select.CountAsync(),
            CategoriesCount = await _categoryRepo.Select.CountAsync(),
            PhotosCount = await _photoRepo.Select.CountAsync(),
            FeaturedPostsCount = await _fPostRepo.Select.CountAsync(),
            FeaturedCategoriesCount = await _fCategoryRepo.Select.CountAsync(),
            FeaturedPhotosCount = await _fPhotoRepo.Select.CountAsync()
        };
    }

    public async Task<Post?> GetTopOnePost() {
        return (await _topPostRepo.Select.Include(a => a.Post.Category).FirstAsync())?.Post;
    }

    /// <summary>
    /// 获取推荐博客row，一行最多两个博客
    /// </summary>
    /// <returns></returns>
    [Obsolete("不需要分出来row了，直接使用 GetFeaturedPosts() 即可")]
    public async Task<List<List<Post>>> GetFeaturedPostRows() {
        var data = new List<List<Post>>();

        var posts = await GetFeaturedPosts();
        for (var i = 0; i < posts.Count; i += 2) {
            data.Add(new List<Post> {posts[i], posts[i + 1]});
        }

        return data;
    }

    public async Task<List<Post>> GetFeaturedPosts() {
        return await _fPostRepo.Select.Include(a => a.Post.Category)
            .ToListAsync(a => a.Post);
    }

    public async Task<FeaturedPost> AddFeaturedPost(Post post) {
        var item = await _fPostRepo.Where(a => a.PostId == post.Id).FirstAsync();
        if (item != null) return item;
        item = new FeaturedPost {PostId = post.Id};
        await _fPostRepo.InsertAsync(item);
        return item;
    }

    public async Task<int> DeleteFeaturedPost(Post post) {
        var items = await _fPostRepo.Where(a => a.PostId == post.Id).CountAsync();
        return items == 0 ? 0 : await _fPostRepo.Where(a => a.PostId == post.Id).ToDelete().ExecuteAffrowsAsync();
    }

    /// <summary>
    /// 设置置顶博客
    /// </summary>
    /// <param name="post"></param>
    /// <returns>返回 <see cref="TopPost"/> 对象和删除原有置顶博客的行数</returns>
    public async Task<(TopPost, int)> SetTopPost(Post post) {
        var rows = await _topPostRepo.Select.ToDelete().ExecuteAffrowsAsync();
        var item = new TopPost {PostId = post.Id};
        await _topPostRepo.InsertAsync(item);
        return (item, rows);
    }

    /// <summary>
    /// 获取文章的状态列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<string?>> GetStatusList() {
        return await _postRepo.Select.GroupBy(a => a.Status)
            .ToListAsync(a => a.Key);
    }

    /// <summary>
    /// 上传博客
    /// todo 初步完成了这个功能，但太多冗余代码了，需要优化
    /// </summary>
    /// <returns></returns>
    public async Task<Post> Upload(PostCreationDto dto, IFormFile file) {
        var tempFile = Path.GetTempFileName();
        await using (var fs = new FileStream(tempFile, FileMode.Create)) {
            await file.CopyToAsync(fs);
        }

        var extractPath = Path.Combine(Path.GetTempPath(), "StarBlog", Guid.NewGuid().ToString());

        // 使用指定编码解压，防止中文文件名乱码
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ZipFile.ExtractToDirectory(tempFile, extractPath, Encoding.GetEncoding(dto.ZipEncoding));

        var dir = new DirectoryInfo(extractPath);
        var files = dir.GetFiles("*.md");
        var mdFile = files.First();

        using var reader = mdFile.OpenText();
        var content = await reader.ReadToEndAsync();
        var post = new Post {
            Id = GuidUtils.GuidTo16String(),
            Status = "已发布",
            Title = dto.Title ?? $"{DateTime.Now.ToLongDateString()} 文章",
            Summary = dto.Summary,
            IsPublish = true,
            Content = content,
            Path = "",
            CreationTime = DateTime.Now,
            LastUpdateTime = DateTime.Now,
            CategoryId = dto.CategoryId,
        };

        // 处理多级分类
        var category = await _categoryRepo.Where(a => a.Id == dto.CategoryId).FirstAsync();
        if (category == null) {
            post.Categories = "0";
        }
        else {
            var categories = new List<Category> {category};
            var parent = category.Parent;
            while (parent != null) {
                categories.Add(parent);
                parent = parent.Parent;
            }

            categories.Reverse();
            post.Categories = string.Join(",", categories.Select(a => a.Id));
        }

        var assetsPath = Path.Combine(_environment.WebRootPath, "media", "blog");
        var processor = new PostProcessor(extractPath, assetsPath, post);

        // 处理文章标题和状态
        processor.InflateStatusTitle();

        // 处理文章正文内容
        // 导入文章的时候一并导入文章里的图片，并对图片相对路径做替换操作
        post.Content = processor.MarkdownParse();
        if (string.IsNullOrEmpty(post.Summary)) {
            post.Summary = processor.GetSummary(200);
        }

        // 存入数据库
        post = await _postRepo.InsertAsync(post);

        return post;
    }
}