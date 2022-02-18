using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;
using X.PagedList;

namespace StarBlog.Web.Controllers;

public class BlogController : Controller {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly PostService _postService;

    public BlogController(IBaseRepository<Post> postRepo, IBaseRepository<Category> categoryRepo, PostService postService) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _postService = postService;
    }

    public IActionResult List(int categoryId = 0, int page = 1, int pageSize = 5) {
        var categories = _categoryRepo.Select.IncludeMany(a => a.Posts).ToList();
        categories.Insert(0, new Category {Id = 0, Name = "All", Posts = _postRepo.Select.ToList()});
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

        return View(new BlogListViewModel {
            CurrentCategory = categoryId == 0 ? categories[0] : categories.First(a => a.Id == categoryId),
            CurrentCategoryId = categoryId,
            Categories = categories,
            Posts = posts.ToPagedList(page, pageSize)
        });
    }

    public IActionResult Post(string id) {
        return View(_postService.GetPostViewModel(_postRepo.Where(a => a.Id == id)
            .Include(a => a.Category)
            .First()));
    }

    public IActionResult Search(string keyword, int categoryId = 0, int page = 1, int pageSize = 5) {
        var posts = _postRepo
            .Where(a => a.Title.Contains(keyword))
            .Include(a => a.Category)
            .ToList();
        var categories = posts.Select(a => a.Category).ToList();
        // todo 搜索不能简单粗暴返回 BlogListViewModel ！ 需要另起页面！
        return View("List", new BlogListViewModel {
        });
    }
}