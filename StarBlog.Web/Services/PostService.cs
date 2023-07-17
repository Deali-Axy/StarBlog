using System.Net;
using FreeSql;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using StarBlog.Share.Utils;
using StarBlog.Data.Models;
using StarBlog.Share.Extensions.Markdown;
using StarBlog.Web.ViewModels;
using StarBlog.Web.ViewModels.QueryFilters;
using X.PagedList;

namespace StarBlog.Web.Services;

public class PostService {
    private readonly ILogger<PostService> _logger;
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _accessor;
    private readonly LinkGenerator _generator;
    private readonly ConfigService _conf;
    private readonly CommonService _commonService;

    private string Host => _conf["host"];

    public PostService(IBaseRepository<Post> postRepo,
        IBaseRepository<Category> categoryRepo,
        IWebHostEnvironment environment,
        IHttpContextAccessor accessor,
        LinkGenerator generator,
        ConfigService conf,
        CommonService commonService,
        ILogger<PostService> logger) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _environment = environment;
        _accessor = accessor;
        _generator = generator;
        _conf = conf;
        _commonService = commonService;
        _logger = logger;
    }

    public async Task<Post?> GetById(string id) {
        // 获取文章的时候对markdown中的图片地址解析，加上完整地址返回给前端
        var post = await _postRepo.Where(a => a.Id == id).Include(a => a.Category).FirstAsync();
        if (post != null) post.Content = MdImageLinkConvert(post, true);

        return post;
    }

    public async Task<int> Delete(string id) {
        return await _postRepo.DeleteAsync(a => a.Id == id);
    }

    public async Task<Post> InsertOrUpdateAsync(Post post) {
        var postId = post.Id;
        // 是新文章的话，先保存到数据库
        if (await _postRepo.Where(a => a.Id == postId).CountAsync() == 0) {
            post = await _postRepo.InsertAsync(post);
        }

        // 检查文章中的外部图片，下载并进行替换
        // todo 将外部图片下载放到异步任务中执行，以免保存文章的时候太慢
        post.Content = await MdExternalUrlDownloadAsync(post);
        // 修改文章时，将markdown中的图片地址替换成相对路径再保存
        post.Content = MdImageLinkConvert(post, false);

        // 处理完内容再更新一次
        await _postRepo.UpdateAsync(post);
        return post;
    }

    /// <summary>
    /// 指定文章上传图片
    /// </summary>
    public async Task<string> UploadImage(Post post, IFormFile file) {
        InitPostMediaDir(post);

        // 直接生成唯一文件名，不保留原始文件名了。——2023-6-5 21:21:46
        var filename = GuidUtils.GuidTo16String() + Path.GetExtension(file.FileName);
        var fileRelativePath = Path.Combine("media", "blog", post.Id, filename);
        var savePath = Path.Combine(_environment.WebRootPath, fileRelativePath);

        await using (var fs = new FileStream(savePath, FileMode.Create)) {
            await file.CopyToAsync(fs);
        }

        return Path.Combine(Host, fileRelativePath);
    }

    /// <summary>
    /// 获取指定文章的图片资源
    /// </summary>
    public List<string> GetImages(Post post) {
        var data = new List<string>();
        var postDir = InitPostMediaDir(post);
        foreach (var file in Directory.GetFiles(postDir)) {
            data.Add(Path.Combine(Host, "media", "blog", post.Id, Path.GetFileName(file)));
        }

        return data;
    }

    public async Task<IPagedList<Post>> GetPagedList(PostQueryParameters param) {
        var querySet = _postRepo.Select;

        // 是否发布
        if (param.OnlyPublished) {
            querySet = _postRepo.Select.Where(a => a.IsPublish);
        }

        // 状态过滤
        if (!string.IsNullOrWhiteSpace(param.Status)) {
            querySet = querySet.Where(a => a.Status == param.Status);
        }

        // 分类过滤
        if (param.CategoryId != 0) {
            querySet = querySet.Where(a => a.CategoryId == param.CategoryId);
        }

        // 关键词过滤
        if (!string.IsNullOrWhiteSpace(param.Search)) {
            querySet = querySet.Where(a => a.Title.Contains(param.Search));
        }

        // 排序
        if (!string.IsNullOrWhiteSpace(param.SortBy)) {
            // 是否升序
            var isAscending = !param.SortBy.StartsWith("-");
            var orderByProperty = param.SortBy.Trim('-');

            querySet = querySet.OrderByPropertyName(orderByProperty, isAscending);
        }

        return (await querySet.Include(a => a.Category).ToListAsync())
            .ToPagedList(param.Page, param.PageSize);
    }

    /// <summary>
    /// 将 Post 对象转换为 PostViewModel 对象
    /// </summary>
    public async Task<PostViewModel> GetPostViewModel(Post post, bool md2Html = true) {
        var model = new PostViewModel {
            Id = post.Id,
            Title = post.Title,
            Summary = post.Summary ?? "（没有介绍）",
            Content = post.Content ?? "（没有内容）",
            Path = post.Path ?? string.Empty,
            Url = _generator.GetUriByAction(
                _accessor.HttpContext!,
                "Post", "Blog",
                new {post.Id}
            ),
            CreationTime = post.CreationTime,
            LastUpdateTime = post.LastUpdateTime,
            Category = post.Category,
            Categories = new List<Category>(),
            TocNodes = post.ExtractToc()
        };

        if (!string.IsNullOrWhiteSpace(post.Slug)) {
            model.Url = Host + _generator.GetPathByAction(
                _accessor.HttpContext!,
                "PostBySlug", "Blog",
                new {post.Slug}
            );
        }

        if (md2Html) {
            model.ContentHtml = GetContentHtml(post);
        }

        if (post.Categories != null) {
            foreach (var itemId in post.Categories.Split(",").Select(int.Parse)) {
                var item = await _categoryRepo.Where(a => a.Id == itemId).FirstAsync();
                if (item != null) model.Categories.Add(item);
            }
        }

        return model;
    }

    public static string GetContentHtml(Post post) {
        // todo 研究一下后端渲染Markdown (PS: 虽然前端渲染轮子更多、效果更好，但后端渲染不会有割裂感）
        // 这部分一些参考资料：
        // - 关于前端渲染 MarkDown 样式：https://blog.csdn.net/sprintline/article/details/122849907
        // - https://github.com/showdownjs/showdown
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseBootstrap5()
            .Build();
        return Markdown.ToHtml(post.Content ?? "", pipeline);
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
    /// <para>支持添加或去除Markdown中的图片URL前缀</para>
    /// </summary>
    /// <param name="post"></param>
    /// <param name="isAddPrefix">是否添加本站的完整URL前缀</param>
    /// <returns></returns>
    private string MdImageLinkConvert(Post post, bool isAddPrefix = true) {
        if (post.Content == null) return string.Empty;
        var document = Markdown.Parse(post.Content);

        foreach (var node in document.AsEnumerable()) {
            if (node is not ParagraphBlock {Inline: { }} paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline) {
                if (inline is not LinkInline {IsImage: true} linkInline) continue;

                var imgUrl = linkInline.Url;
                if (imgUrl == null) continue;

                // 已有Host前缀，跳过
                if (isAddPrefix && imgUrl.StartsWith(Host)) continue;

                // 设置完整链接
                if (isAddPrefix) {
                    if (imgUrl.StartsWith("http")) continue;
                    linkInline.Url = $"{Host}/media/blog/{post.Id}/{imgUrl}";
                }
                // 设置成相对链接
                else {
                    linkInline.Url = Path.GetFileName(imgUrl);
                }
            }
        }

        using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }

    /// <summary>
    /// Markdown中外部图片下载
    /// <para>如果Markdown中包含外部图片URL，则下载到本地且进行URL替换</para>
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    private async Task<string> MdExternalUrlDownloadAsync(Post post) {
        if (post.Content == null) return string.Empty;

        // 得先初始化目录
        InitPostMediaDir(post);

        var document = Markdown.Parse(post.Content);
        foreach (var node in document.AsEnumerable()) {
            if (node is not ParagraphBlock {Inline: { }} paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline) {
                if (inline is not LinkInline {IsImage: true} linkInline) continue;

                var imgUrl = linkInline.Url;
                // 跳过空链接
                if (imgUrl == null) continue;
                // 跳过本站地址的图片
                if (imgUrl.StartsWith(Host)) continue;

                // 下载图片
                _logger.LogDebug("文章：{Title}，下载图片：{Url}", post.Title, imgUrl);
                var savePath = Path.Combine(_environment.WebRootPath, "media", "blog", post.Id!);
                var fileName = await _commonService.DownloadFileAsync(imgUrl, savePath);
                linkInline.Url = fileName;
            }
        }

        await using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }
}