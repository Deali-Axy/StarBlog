using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels;

namespace StarBlog.Web.Controllers;

public class SearchController : Controller {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;

    public SearchController(IBaseRepository<Post> postRepo, IBaseRepository<Category> categoryRepo) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
    }

    public IActionResult Blog(string keyword, int categoryId = 0, int page = 1, int pageSize = 5) {
        var posts = _postRepo
            .Where(a => a.IsPublish)
            .Where(a => a.Title!.Contains(keyword))
            .Include(a => a.Category)
            .ToList();

        return View("Result", new SearchResultViewModel {
            Keyword = keyword,
            Posts = posts
        });
    }
}