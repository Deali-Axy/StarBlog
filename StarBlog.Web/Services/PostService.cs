using FreeSql;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using StarBlog.Contrib.Utils;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels;
using X.PagedList;

namespace StarBlog.Web.Services;

public class PostService {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public string Host => _configuration.GetSection("Server:Host").Value;

    public PostService(IBaseRepository<Post> postRepo,
        IBaseRepository<Category> categoryRepo,
        IWebHostEnvironment environment,
        IConfiguration configuration) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _environment = environment;
        _configuration = configuration;
    }

    public Post? GetById(string id) {
        // 获取文章的时候对markdown中的图片地址解析，加上完整地址返回给前端
        var post = _postRepo.Where(a => a.Id == id).Include(a => a.Category).First();
        post.Content = MdImageLinkConvert(post, true);
        return post;
    }

    public int Delete(string id) {
        return _postRepo.Delete(a => a.Id == id);
    }

    public Post InsertOrUpdate(Post post) {
        // 修改文章时，将markdown中的图片地址替换成相对路径再保存
        post.Content = MdImageLinkConvert(post, false);
        return _postRepo.InsertOrUpdate(post);
    }

    /// <summary>
    /// 指定文章上传图片
    /// </summary>
    /// <param name="post"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public string UploadImage(Post post, IFormFile file) {
        InitPostMediaDir(post);

        var fileRelativePath = Path.Combine("media", "blog", post.Id, file.FileName);
        var savePath = Path.Combine(_environment.WebRootPath, fileRelativePath);
        if (File.Exists(savePath)) {
            // 上传文件重名处理
            var newFilename = $"{Path.GetFileNameWithoutExtension(file.FileName)}-{GuidUtils.GuidTo16String()}.{Path.GetExtension(file.FileName)}";
            fileRelativePath = Path.Combine("media", "blog", post.Id, newFilename);
            savePath = Path.Combine(_environment.WebRootPath, fileRelativePath);
        }

        using (var fs = new FileStream(savePath, FileMode.Create)) {
            file.CopyTo(fs);
        }

        return Path.Combine(Host, fileRelativePath);
    }

    /// <summary>
    /// 获取指定文章的图片资源
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    public List<string> GetImages(Post post) {
        var data = new List<string>();
        var postDir = InitPostMediaDir(post);
        foreach (var file in Directory.GetFiles(postDir)) {
            data.Add(Path.Combine(Host, "media", "blog", post.Id, Path.GetFileName(file)));
        }

        return data;
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
        var vm = new PostViewModel {
            Id = post.Id,
            Title = post.Title,
            Summary = post.Summary,
            Content = post.Content,
            ContentHtml = Markdig.Markdown.ToHtml(post.Content),
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

    /// <summary>
    /// 初始化博客文章的资源目录
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    private string InitPostMediaDir(Post post) {
        var blogMediaDir = Path.Combine(_environment.WebRootPath, "media", "blog");
        var postMediaDir = Path.Combine(_environment.WebRootPath, "media", "blog", post.Id);
        if (!Directory.Exists(blogMediaDir)) Directory.CreateDirectory(blogMediaDir);
        if (!Directory.Exists(postMediaDir)) Directory.CreateDirectory(postMediaDir);

        return postMediaDir;
    }

    /// <summary>
    /// Markdown中的图片链接转换
    /// </summary>
    /// <param name="post"></param>
    /// <param name="isAddPrefix"></param>
    /// <returns></returns>
    private string MdImageLinkConvert(Post post, bool isAddPrefix = true) {
        var document = Markdown.Parse(post.Content);

        foreach (var node in document.AsEnumerable()) {
            if (node is not ParagraphBlock { Inline: { } } paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline) {
                if (inline is not LinkInline { IsImage: true } linkInline) continue;

                var imgUrl = linkInline.Url;
                if (imgUrl == null) continue;
                if (isAddPrefix && imgUrl.StartsWith("http")) continue;
                if (isAddPrefix) {
                    if(imgUrl.StartsWith("http")) continue;
                    // 设置完整链接
                    linkInline.Url = $"{Host}/media/blog/{post.Id}/{imgUrl}";
                }
                else {
                    // 设置成相对链接
                    linkInline.Url = Path.GetFileName(imgUrl);
                }
            }
        }

        using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }
}