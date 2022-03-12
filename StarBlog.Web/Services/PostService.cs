using FreeSql;
using Markdig;
using Markdown.ColorCode;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels;
using X.PagedList;

namespace StarBlog.Web.Services;

public class PostService {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly IWebHostEnvironment _environment;


    public PostService(IBaseRepository<Post> postRepo, IBaseRepository<Category> categoryRepo, IWebHostEnvironment environment) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _environment = environment;
    }

    public Post? GetById(string id) {
        return _postRepo.Where(a => a.Id == id).Include(a => a.Category).First();
    }

    public int Delete(string id) {
        return _postRepo.Delete(a => a.Id == id);
    }

    public Post InsertOrUpdate(Post post) {
        return _postRepo.InsertOrUpdate(post);
    }

    public string UploadImage(Post post, IFormFile file) {
        var blogMediaDir = Path.Combine(_environment.WebRootPath, "media", "blog");
        var postMediaDir = Path.Combine(_environment.WebRootPath, "media", "blog", post.Id);
        if (!Directory.Exists(blogMediaDir)) Directory.CreateDirectory(blogMediaDir);
        if (!Directory.Exists(postMediaDir)) Directory.CreateDirectory(postMediaDir);

        var fileRelativePath = Path.Combine("media", "blog", post.Id, file.FileName);
        var savePath = Path.Combine(_environment.WebRootPath, fileRelativePath);
        using (var fs = new FileStream(savePath, FileMode.Create)) {
            file.CopyTo(fs);
        }

        return Path.Combine("http://127.0.0.1:5038", fileRelativePath);
    }

    public IPagedList<Post> GetPagedList(int categoryId = 0, int page = 1, int pageSize = 10) {
        List<Post> posts;
        if (categoryId == 0) {
            posts = _postRepo.Select
                .OrderByDescending(a => a.LastUpdateTime)
                .Include(a => a.Category)
                .ToList();
        }
        else {
            posts = _postRepo.Where(a => a.CategoryId == categoryId)
                .OrderByDescending(a => a.LastUpdateTime)
                .Include(a => a.Category)
                .ToList();
        }

        return posts.ToPagedList(page, pageSize);
    }

    /// <summary>
    /// 将 Post 对象转换为 PostViewModel 对象
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    public PostViewModel GetPostViewModel(Post post) {
        var mdPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UsePipeTables()
            .UseEmphasisExtras()
            .UseGenericAttributes()
            .UseDefinitionLists()
            .UseAutoIdentifiers()
            .UseAutoLinks()
            .UseTaskLists()
            .UseBootstrap()
            // .UseColorCode()
            .Build();

        var vm = new PostViewModel {
            Id = post.Id,
            Title = post.Title,
            Summary = post.Summary,
            Content = post.Content,
            ContentHtml = Markdig.Markdown.ToHtml(post.Content, mdPipeline),
            Path = post.Path,
            CreationTime = post.CreationTime,
            LastUpdateTime = post.LastUpdateTime,
            Category = post.Category,
            Categories = new List<Category>()
        };

        foreach (var itemId in post.Categories.Split(",").Select(int.Parse)) {
            var item = _categoryRepo.Where(a => a.Id == itemId).First();
            if (item != null) vm.Categories.Add(item);
        }

        return vm;
    }
}