using System.Net;
using FreeSql;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using StarBlog.Content.Utils;
using StarBlog.Data.Models;
using StarBlog.Content.Extensions.Markdown;
using StarBlog.Application.Abstractions;
using StarBlog.Application.Criteria;
using X.PagedList;

namespace StarBlog.Application.Services;

public class PostService {
    private readonly ILogger<PostService> _logger;
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly IAppPathProvider _paths;
    private readonly IFileStorage _fileStorage;
    private readonly ConfigService _conf;
    private readonly CommonService _commonService;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IServiceScopeFactory _scopeFactory;

    private string Host => _conf["host"];

    public PostService(IBaseRepository<Post> postRepo,
        IBaseRepository<Category> categoryRepo,
        IAppPathProvider paths,
        IFileStorage fileStorage,
        ConfigService conf,
        CommonService commonService,
        IBackgroundTaskQueue backgroundTaskQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<PostService> logger) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _paths = paths;
        _fileStorage = fileStorage;
        _conf = conf;
        _commonService = commonService;
        _backgroundTaskQueue = backgroundTaskQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// 检查 slug 是否可用
    /// </summary>
    public async Task<bool> CheckSlugAvailable(string slug) {
        return !await _postRepo.Select.AnyAsync(a => a.Slug == slug);
    }

    public async Task<Post?> GetById(string id) {
        // 获取文章的时候对markdown中的图片地址解析，加上完整地址返回给前端
        var post = await _postRepo.Where(a => a.Id == id).Include(a => a.Category).FirstAsync();
        if (post != null) post.Content = MdImageLinkConvert(post, true);

        return post;
    }

    public async Task<Post?> GetBySlug(string slug) {
        var post = await _postRepo.Where(a => a.Slug == slug).FirstAsync();
        if (post != null) post.Content = MdImageLinkConvert(post, true);
        return post;
    }

    public async Task<int> Delete(string id) {
        // todo 删除文章的时候顺便删除对应的图片资源
        return await _postRepo.DeleteAsync(a => a.Id == id);
    }

    public async Task<Post> InsertOrUpdateAsync(Post post) {
        var postId = post.Id;
        // 是新文章的话，先保存到数据库
        if (await _postRepo.Where(a => a.Id == postId).CountAsync() == 0) {
            post = await _postRepo.InsertAsync(post);
        }

        if (ContainsExternalImages(post)) {
            var postIdForJob = post.Id;
            await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async cancellationToken => {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<PostService>();
                await service.LocalizeExternalImagesAsync(postIdForJob, cancellationToken);
            });
        }

        // 修改文章时，将markdown中的本站图片地址替换成相对路径再保存
        post.Content = MdImageLinkConvert(post, false);
        // todo 修改文章时，要同时重新生成分类层级


        // 处理完内容再更新一次
        await _postRepo.UpdateAsync(post);
        return post;
    }

    public async Task LocalizeExternalImagesAsync(string postId, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(postId)) return;

        var post = await _postRepo.Where(a => a.Id == postId).FirstAsync();
        if (post == null) return;
        if (!ContainsExternalImages(post)) return;

        post.Content = await MdExternalUrlDownloadAsync(post);
        post.Content = MdImageLinkConvert(post, false);
        post.LastUpdateTime = DateTime.Now;

        cancellationToken.ThrowIfCancellationRequested();
        await _postRepo.UpdateAsync(post);
    }

    /// <summary>
    /// 指定文章上传图片
    /// </summary>
    public async Task<string> UploadImage(Post post, Stream fileStream, string originalFileName) {
        await _fileStorage.EnsureDirectoryAsync(Path.Combine("media", "blog", post.Id));

        // 直接生成唯一文件名，不保留原始文件名了。——2023-6-5 21:21:46
        var filename = GuidUtils.GuidTo16String() + Path.GetExtension(originalFileName);
        var fileRelativePath = Path.Combine("media", "blog", post.Id, filename);
        await _fileStorage.SaveAsync(fileRelativePath, fileStream);
        return $"{Host}/{fileRelativePath.Replace('\\', '/')}";
    }

    /// <summary>
    /// 获取指定文章的图片资源
    /// </summary>
    public List<string> GetImages(Post post) {
        var data = new List<string>();
        var files = _fileStorage.ListFilesAsync(Path.Combine("media", "blog", post.Id)).GetAwaiter().GetResult();
        foreach (var file in files) {
            data.Add($"{Host}/media/blog/{post.Id}/{file}");
        }

        return data;
    }

    public async Task<IPagedList<Post>> GetPagedList(PostQueryParameters param, bool adminMode = false) {
        var querySet = _postRepo.Select;

        // 筛选发布状态
        // 仅管理员能筛选发布状态
        if (param.IsPublish != null && adminMode) {
            querySet = _postRepo.Select.Where(a => a.IsPublish == param.IsPublish);
        }

        if (!adminMode) {
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

        IPagedList<Post> pagedList = new StaticPagedList<Post>(
            await querySet.Page(param.Page, param.PageSize).Include(a => a.Category).ToListAsync(),
            param.Page, param.PageSize, Convert.ToInt32(await querySet.CountAsync())
        );
        return pagedList;
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
        var blogMediaDir = Path.Combine(_paths.WebRootPath, "media", "blog");
        var postMediaDir = Path.Combine(_paths.WebRootPath, "media", "blog", post.Id);
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
            if (node is not ParagraphBlock { Inline: { } } paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline) {
                if (inline is not LinkInline { IsImage: true } linkInline) continue;

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
                    if (imgUrl.StartsWith(Host)) {
                        linkInline.Url = Path.GetFileName(imgUrl);
                    }
                }
            }
        }

        using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }

    private bool ContainsExternalImages(Post post) {
        if (post.Content == null) return false;

        var document = Markdown.Parse(post.Content);
        foreach (var node in document.AsEnumerable()) {
            if (node is not ParagraphBlock { Inline: { } } paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline) {
                if (inline is not LinkInline { IsImage: true } linkInline) continue;

                var imgUrl = linkInline.Url;
                if (imgUrl == null) continue;
                if (imgUrl.StartsWith(Host)) continue;
                if (imgUrl.StartsWith("./") || imgUrl.StartsWith("../") || imgUrl.StartsWith("/")) continue;
                if (!imgUrl.StartsWith("http://") && !imgUrl.StartsWith("https://")) continue;

                return true;
            }
        }

        return false;
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
            if (node is not ParagraphBlock { Inline: { } } paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline) {
                if (inline is not LinkInline { IsImage: true } linkInline) continue;

                var imgUrl = linkInline.Url;
                // 跳过空链接
                if (imgUrl == null) continue;
                // 跳过本站地址的图片
                if (imgUrl.StartsWith(Host)) continue;
                // 跳过相对路径和本地路径
                if (imgUrl.StartsWith("./") || imgUrl.StartsWith("../") || imgUrl.StartsWith("/")) continue;
                // 跳过非HTTP协议的链接（如file://等）
                if (!imgUrl.StartsWith("http://") && !imgUrl.StartsWith("https://")) continue;

                // 下载图片
                _logger.LogDebug("文章：{Title}，下载图片：{Url}", post.Title, imgUrl);
                var savePath = Path.Combine(_paths.WebRootPath, "media", "blog", post.Id!);
                var fileName = await _commonService.DownloadFileAsync(imgUrl, savePath);
                linkInline.Url = fileName;
            }
        }

        await using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }

    /// <summary>
    /// 获取相关文章推荐（随机顺序）
    /// </summary>
    public async Task<List<Post>> GetRelatedPosts(Post currentPost, int count = 5) {
        var relatedPosts = new List<Post>();

        // 1. 优先推荐同分类的文章（随机顺序）
        if (currentPost.CategoryId > 0) {
            var sameCategoryPosts = await _postRepo
                .Where(p => p.IsPublish && p.Id != currentPost.Id && p.CategoryId == currentPost.CategoryId)
                .ToListAsync();

            // 随机打乱同分类文章
            var randomSameCategoryPosts = sameCategoryPosts
                .OrderBy(x => Random.Shared.Next())
                .Take(count)
                .ToList();
            relatedPosts.AddRange(randomSameCategoryPosts);
        }

        // 2. 如果同分类文章不够，从其他分类补充（随机顺序）
        if (relatedPosts.Count < count) {
            var remainingCount = count - relatedPosts.Count;
            var excludeIds = relatedPosts.Select(r => r.Id).ToList();
            excludeIds.Add(currentPost.Id); // 排除当前文章

            var otherPosts = await _postRepo
                .Where(p => p.IsPublish && !excludeIds.Contains(p.Id))
                .ToListAsync();

            // 随机打乱其他文章
            var randomOtherPosts = otherPosts
                .OrderBy(x => Random.Shared.Next())
                .Take(remainingCount)
                .ToList();
            relatedPosts.AddRange(randomOtherPosts);
        }

        return relatedPosts.Take(count).ToList();
    }

    /// <summary>
    /// 获取热门文章
    /// </summary>
    public async Task<List<Post>> GetPopularPosts(int count = 10) {
        return await _postRepo
            .Where(p => p.IsPublish)
            .OrderByDescending(p => p.LastUpdateTime)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// 获取最新文章
    /// </summary>
    public async Task<List<Post>> GetLatestPosts(int count = 10) {
        return await _postRepo
            .Where(p => p.IsPublish)
            .OrderByDescending(p => p.CreationTime)
            .Take(count)
            .ToListAsync();
    }
}
