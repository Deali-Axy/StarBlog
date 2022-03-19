using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Contrib.SiteMessage;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;
using StarBlog.Web.ViewModels.QueryFilters;
using X.PagedList;

namespace StarBlog.Web.Controllers;

public class BlogController : Controller {
    private readonly Messages _messages;
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly PostService _postService;

    public BlogController(IBaseRepository<Post> postRepo, IBaseRepository<Category> categoryRepo, PostService postService,
        Messages messages) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _postService = postService;
        _messages = messages;
    }

    public IActionResult List(int categoryId = 0, int page = 1, int pageSize = 5) {
        var categories = _categoryRepo.Where(a => a.Visible)
            .IncludeMany(a => a.Posts).ToList();
        categories.Insert(0, new Category {Id = 0, Name = "All", Posts = _postRepo.Select.ToList()});

        return View(new BlogListViewModel {
            CurrentCategory = categoryId == 0 ? categories[0] : categories.First(a => a.Id == categoryId),
            CurrentCategoryId = categoryId,
            Categories = categories,
            Posts = _postService.GetPagedList(new PostQueryParameters {
                CategoryId = categoryId,
                Page = page,
                PageSize = pageSize
            })
        });
    }

    public IActionResult Post(string id) {
        return View(_postService.GetPostViewModel(_postRepo.Where(a => a.Id == id)
            .Include(a => a.Category)
            .First()));
    }

    public IActionResult RandomPost() {
        var posts = _postRepo.Select.ToList();
        var rndPost = posts[new Random().Next(posts.Count)];
        _messages.Info($"随机推荐了文章 <b>{rndPost.Title}</b> 给你~");
        return RedirectToAction(nameof(Post), new {id = rndPost.Id});
    }
}