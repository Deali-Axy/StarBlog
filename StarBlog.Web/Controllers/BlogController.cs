using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Contrib.SiteMessage;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Blog;
using StarBlog.Web.ViewModels.QueryFilters;
using X.PagedList;

namespace StarBlog.Web.Controllers;

public class BlogController : Controller {
    private readonly Messages _messages;
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly PostService _postService;
    private readonly CategoryService _categoryService;

    public BlogController(IBaseRepository<Post> postRepo,
        IBaseRepository<Category> categoryRepo,
        PostService postService,
        Messages messages,
        CategoryService categoryService) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _postService = postService;
        _messages = messages;
        _categoryService = categoryService;
    }

    public IActionResult List(int categoryId = 0, int page = 1, int pageSize = 5) {
        var categories = _categoryRepo.Where(a => a.Visible)
            .IncludeMany(a => a.Posts).ToList();
        categories.Insert(0, new Category {Id = 0, Name = "All", Posts = _postRepo.Select.ToList()});

        var currentCategory = categoryId == 0 ? categories[0] : _categoryService.GetById(categoryId);

        if (currentCategory == null) {
            _messages.Error($"分类 {categoryId} 不存在！");
            return RedirectToAction(nameof(List));
        }

        if (!currentCategory.Visible) {
            _messages.Warning($"分类 {categoryId} 暂不开放！");
            return RedirectToAction(nameof(List));
        }

        return View(new BlogListViewModel {
            CurrentCategory = categoryId == 0 ? categories[0] : categories.First(a => a.Id == categoryId),
            CurrentCategoryId = categoryId,
            Categories = categories,
            CategoryNodes = _categoryService.GetNodes(),
            Posts = _postService.GetPagedList(new PostQueryParameters {
                CategoryId = categoryId,
                Page = page,
                PageSize = pageSize,
                OnlyPublished = true
            })
        });
    }

    public IActionResult Post(string id) {
        var post = _postService.GetById(id);

        if (post == null) {
            _messages.Error($"文章 {id} 不存在！");
            return RedirectToAction(nameof(List));
        }

        if (!post.IsPublish) {
            _messages.Warning($"文章 {id} 未发布！");
            return RedirectToAction(nameof(List));
        }

        return View(_postService.GetPostViewModel(post));
    }

    public IActionResult RandomPost() {
        var posts = _postRepo.Where(a => a.IsPublish).ToList();
        if (posts.Count == 0) {
            _messages.Error("当前没有文章，请先添加文章！");
            return RedirectToAction("Index", "Home");
        }

        var rndPost = posts[new Random().Next(posts.Count)];
        _messages.Info($"随机推荐了文章 <b>{rndPost.Title}</b> 给你~");
        return RedirectToAction(nameof(Post), new {id = rndPost.Id});
    }

    public IActionResult Temp() {
        return View();
    }
}