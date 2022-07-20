using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Contrib.SiteMessage;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;

namespace StarBlog.Web.Controllers;

public class HomeController : Controller {
    private readonly BlogService _blogService;
    private readonly PhotoService _photoService;
    private readonly CategoryService _categoryService;
    private readonly LinkService _linkService;
    private readonly Messages _messages;

    public HomeController(BlogService blogService, PhotoService photoService, CategoryService categoryService, LinkService linkService, Messages messages) {
        _blogService = blogService;
        _photoService = photoService;
        _categoryService = categoryService;
        _linkService = linkService;
        _messages = messages;
    }

    public IActionResult Index() {
        return View(new HomeViewModel {
            RandomPhoto = _photoService.GetRandomPhoto(),
            TopPost = _blogService.GetTopOnePost(),
            FeaturedPosts = _blogService.GetFeaturedPosts(),
            FeaturedPhotos = _photoService.GetFeaturedPhotos(),
            FeaturedCategories = _categoryService.GetFeaturedCategories(),
            Links = _linkService.GetAll()
        });
    }

    [HttpGet]
    public IActionResult Init([FromServices] ConfigService conf) {
        if (conf["is_init"] == "true") {
            _messages.Error("已经完成初始化！");
            return RedirectToAction(nameof(Index));
        }

        return View(new InitViewModel {
            Host = conf["host"]
        });
    }

    [HttpPost]
    public IActionResult Init([FromServices] ConfigService conf, [FromServices] IBaseRepository<User> userRepo, InitViewModel vm) {
        if (!ModelState.IsValid) return View();

        // 保存配置
        conf["host"] = vm.Host;
        conf["is_init"] = "true";

        // 创建用户
        // todo 这里暂时存储明文密码，后期要换成MD5加密存储
        userRepo.Insert(new User {
            Id = Guid.NewGuid().ToString(),
            Name = vm.Username,
            Password = vm.Password
        });

        _messages.Success("初始化完成！");
        return RedirectToAction(nameof(Index));
    }
}