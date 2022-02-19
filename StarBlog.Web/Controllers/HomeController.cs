using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;

namespace StarBlog.Web.Controllers;

public class HomeController : Controller {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly BlogService _blogService;

    public HomeController(IBaseRepository<Post> postRepo, BlogService blogService) {
        _postRepo = postRepo;
        _blogService = blogService;
    }

    public IActionResult Index() {
        return View(new HomeViewModel {
            TopPost = _blogService.GetTopOnePost(),
            FeaturedPosts = _blogService.GetFeaturedPosts()
        });
    }
}