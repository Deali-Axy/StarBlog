using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Contrib.SiteMessage;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Blog;
using StarBlog.Web.Criteria;
using X.PagedList;
using StarBlog.Web.ViewModels;

namespace StarBlog.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class BlogController : Controller {
    private readonly MessageService _messages;
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly IBaseRepository<PostTranslation> _translationRepo;
    private readonly PostService _postService;
    private readonly CategoryService _categoryService;
    private readonly ConfigService _configService;
    private readonly SeoService _seoService;
    private readonly StructuredDataService _structuredDataService;

    public BlogController(IBaseRepository<Post> postRepo,
        IBaseRepository<Category> categoryRepo,
        IBaseRepository<PostTranslation> translationRepo,
        PostService postService,
        MessageService messages,
        CategoryService categoryService,
        ConfigService configService,
        SeoService seoService,
        StructuredDataService structuredDataService) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _translationRepo = translationRepo;
        _postService = postService;
        _messages = messages;
        _categoryService = categoryService;
        _configService = configService;
        _seoService = seoService;
        _structuredDataService = structuredDataService;
    }

    public async Task<IActionResult> List(int categoryId = 0, int page = 1, int pageSize = 8,
        string sortType = "asc", string sortBy = "-CreationTime") {
        var currentCategory = categoryId == 0
            ? new Category { Id = 0, Name = "All" }
            : await _categoryRepo.Where(a => a.Id == categoryId).FirstAsync();

        if (currentCategory == null) {
            _messages.Error($"分类 {categoryId} 不存在！");
            return RedirectToAction(nameof(List));
        }

        if (!currentCategory.Visible) {
            _messages.Warning($"分类 {categoryId} 暂不开放！");
            return RedirectToAction(nameof(List));
        }

        var posts = await _postService.GetPagedList(new PostQueryParameters {
            CategoryId = categoryId,
            IncludeSubCategory = true,
            Page = page,
            PageSize = pageSize,
            SortBy = sortType == "desc" ? $"-{sortBy}" : sortBy
        });

        // 设置SEO元数据
        if (categoryId > 0) {
            ViewData["SeoMetadata"] = _seoService.GetCategorySeoMetadata(currentCategory, posts.TotalItemCount);
        } else {
            ViewData["SeoMetadata"] = _seoService.GetBlogListSeoMetadata(page);
        }

        return View(new BlogListViewModel {
            CurrentCategory = currentCategory,
            CurrentCategoryId = categoryId,
            CategoryNodes = await _categoryService.GetNodes(),
            SortType = sortType,
            SortBy = sortBy,
            Posts = posts
        });
    }

    [Route("/p/{slug}")]
    public async Task<IActionResult> PostBySlug(string slug, [FromQuery] string? lang) {
        var p = await _postRepo.Where(a => a.Slug == slug).FirstAsync();
        return await Post(p?.Id ?? "", lang);
    }

    public async Task<IActionResult> Post(string id, [FromQuery] string? lang) {
        var post = await _postService.GetById(id);

        if (post == null) {
            _messages.Error($"文章 {id} 不存在！");
            return RedirectToAction(nameof(List));
        }

        if (!post.IsPublish) {
            _messages.Warning($"文章 {id} 未发布！");
            return RedirectToAction(nameof(List));
        }

        var postViewModel = await _postService.GetPostViewModel(post);

        // 查询可用翻译语言
        var translations = await _translationRepo
            .Where(t => t.PostId == post.Id)
            .ToListAsync();
        postViewModel.AvailableLanguages = translations
            .Select(t => t.Language)
            .Distinct()
            .ToList();

        // 如果指定了语言且有翻译，替换内容
        if (!string.IsNullOrEmpty(lang) && lang != "zh") {
            var translation = translations.FirstOrDefault(t => t.Language == lang);
            if (translation != null) {
                postViewModel.Title = translation.Title ?? postViewModel.Title;
                postViewModel.Summary = translation.Summary ?? postViewModel.Summary;
                postViewModel.Content = translation.Content ?? postViewModel.Content;
                postViewModel.CurrentLanguage = lang;
                if (!string.IsNullOrEmpty(translation.Content)) {
                    postViewModel.ContentHtml = PostService.GetContentHtml(translation);
                }
            }
        }

        // 存储语言偏好到 Cookie
        if (!string.IsNullOrEmpty(lang)) {
            Response.Cookies.Append("blog_lang", lang, new CookieOptions {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = false,
                IsEssential = true
            });
        }

        // 设置SEO元数据
        ViewData["SeoMetadata"] = _seoService.GetPostSeoMetadata(postViewModel);

        // 设置结构化数据
        var structuredData = new Dictionary<string, string> {
            ["BlogPosting"] = _structuredDataService.GetBlogPostingStructuredData(postViewModel),
            ["BreadcrumbList"] = _structuredDataService.GetBreadcrumbStructuredData(postViewModel),
            ["Person"] = _structuredDataService.GetPersonStructuredData()
        };
        ViewData["StructuredData"] = structuredData;

        // 获取相关文章
        ViewData["RelatedPosts"] = await _postService.GetRelatedPosts(post, 4);

        var viewName = "Post.FrontendRender";
        if (_configService["default_render"] == "backend") {
            viewName = "Post.BackendRender";
        }

        return View(viewName, postViewModel);
    }

    public IActionResult RandomPost() {
        var posts = _postRepo.Where(a => a.IsPublish).ToList();
        if (posts.Count == 0) {
            _messages.Error("当前没有文章，请先添加文章！");
            return RedirectToAction("Index", "Home");
        }

        var rndPost = posts[Random.Shared.Next(posts.Count)];
        _messages.Info($"随机推荐了文章 <b>{rndPost.Title}</b> 给你~" +
                       $"<span class='ps-3'><a href=\"{Url.Action(nameof(RandomPost))}\">再来一次</a></span>");
        return RedirectToAction(nameof(Post), new { id = rndPost.Id });
    }

    public IActionResult Temp() {
        return View();
    }
}
