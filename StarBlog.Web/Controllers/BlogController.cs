using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels;

namespace StarBlog.Web.Controllers;

public class BlogController : Controller {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;

    public BlogController(IBaseRepository<Post> postRepo, IBaseRepository<Category> categoryRepo) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
    }

    public IActionResult List() {
        return View(new BlogList {
            Posts = _postRepo.Select.Include(a => a.Category).ToList()
        });
    }

    public IActionResult Details(string id) {
        return View(
            _postRepo.Where(a => a.Id == id)
                .Include(a => a.Category)
                .First()
        );
    }
}