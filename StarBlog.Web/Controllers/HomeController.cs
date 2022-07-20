using Microsoft.AspNetCore.Mvc;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;

namespace StarBlog.Web.Controllers;

public class HomeController : Controller {
    private readonly BlogService _blogService;
    private readonly PhotoService _photoService;
    private readonly CategoryService _categoryService;
    private readonly LinkService _linkService;

    public HomeController(BlogService blogService, PhotoService photoService, CategoryService categoryService, LinkService linkService) {
        _blogService = blogService;
        _photoService = photoService;
        _categoryService = categoryService;
        _linkService = linkService;
    }

    public IActionResult Index() {
        if (Request.QueryString.HasValue) {
            return BadRequest();
        }

        return View(new HomeViewModel {
            RandomPhoto = _photoService.GetRandomPhoto(),
            TopPost = _blogService.GetTopOnePost(),
            FeaturedPosts = _blogService.GetFeaturedPosts(),
            FeaturedPhotos = _photoService.GetFeaturedPhotos(),
            FeaturedCategories = _categoryService.GetFeaturedCategories(),
            Links = _linkService.GetAll()
        });
    }
}