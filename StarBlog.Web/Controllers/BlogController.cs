using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Contrib.SiteMessage;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Blog;
using StarBlog.Web.ViewModels.QueryFilters;
using X.PagedList;

namespace StarBlog.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class BlogController : Controller {
    private readonly MessageService _messages;
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly PostService _postService;
    private readonly CategoryService _categoryService;
    private readonly ConfigService _configService;

    public BlogController(IBaseRepository<Post> postRepo,
        IBaseRepository<Category> categoryRepo,
        PostService postService,
        MessageService messages,
        CategoryService categoryService,
        ConfigService configService) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _postService = postService;
        _messages = messages;
        _categoryService = categoryService;
        _configService = configService;
    }

    public async Task<IActionResult> List(int categoryId = 0, int page = 1, int pageSize = 6,
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

        return View(new BlogListViewModel {
            CurrentCategory = currentCategory,
            CurrentCategoryId = categoryId,
            CategoryNodes = await _categoryService.GetNodes(),
            SortType = sortType,
            SortBy = sortBy,
            Posts = await _postService.GetPagedList(new PostQueryParameters {
                CategoryId = categoryId,
                Page = page,
                PageSize = pageSize,
                OnlyPublished = true,
                SortBy = sortType == "desc" ? $"-{sortBy}" : sortBy
            })
        });
    }

    [Route("/p/{slug}")]
    public async Task<IActionResult> PostBySlug(string slug) {
        var p = await _postRepo.Where(a => a.Slug == slug).FirstAsync();
        return await Post(p?.Id ?? "");
    }

    public async Task<IActionResult> Post(string id) {
        var post = await _postService.GetById(id);

        if (post == null) {
            _messages.Error($"文章 {id} 不存在！");
            return RedirectToAction(nameof(List));
        }

        if (!post.IsPublish) {
            _messages.Warning($"文章 {id} 未发布！");
            return RedirectToAction(nameof(List));
        }

        var viewName = "Post.FrontendRender";
        if (_configService["default_render"] == "backend") {
            viewName = "Post.BackendRender";
        }

        return View(viewName, await _postService.GetPostViewModel(post));
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