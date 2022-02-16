using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;

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

    public IActionResult List() {
        return View(new BlogListViewModel {
            Posts = _postRepo.Select.Include(a => a.Category).ToList()
        });
    }

    public IActionResult Details(string id) {
        return View(_postService.GetPostViewModel(_postRepo.Where(a => a.Id == id)
            .Include(a => a.Category)
            .First()));
    }
}