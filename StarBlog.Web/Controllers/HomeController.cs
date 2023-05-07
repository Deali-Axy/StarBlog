using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Contrib.Security;
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

    public HomeController(BlogService blogService, PhotoService photoService, CategoryService categoryService, LinkService linkService,
        Messages messages) {
        _blogService = blogService;
        _photoService = photoService;
        _categoryService = categoryService;
        _linkService = linkService;
        _messages = messages;
    }

    public async Task<IActionResult> Index() {
        if (Request.QueryString.HasValue) {
            return BadRequest();
        }

        return View(new HomeViewModel {
            RandomPhoto = await _photoService.GetRandomPhoto(),
            TopPost = await _blogService.GetTopOnePost(),
            FeaturedPosts = await _blogService.GetFeaturedPosts(),
            FeaturedPhotos = await _photoService.GetFeaturedPhotos(),
            FeaturedCategories = await _categoryService.GetFeaturedCategories(),
            Links = await _linkService.GetAll()
        });
    }

    [HttpGet]
    public IActionResult Init([FromServices] ConfigService conf) {
        if (conf["is_init"] == "true") {
            _messages.Error("已经完成初始化！");
            return RedirectToAction(nameof(Index));
        }

        return View(new InitViewModel {
            Host = conf["host"],
            DefaultRender = conf["default_render"]
        });
    }

    [HttpPost]
    public IActionResult Init([FromServices] ConfigService conf, [FromServices] IBaseRepository<User> userRepo, InitViewModel vm) {
        if (conf["is_init"] == "true") {
            _messages.Error("已经完成初始化！");
            return RedirectToAction(nameof(Index));
        }
        
        if (!ModelState.IsValid) return View();

        // 保存配置
        conf["host"] = vm.Host;
        conf["default_render"] = vm.DefaultRender;
        conf["is_init"] = "true";

        // 创建用户
        // to do 这里暂时存储明文密码，后期要换成MD5加密存储。2023-5-7 搞定
        userRepo.Insert(new User {
            Id = Guid.NewGuid().ToString(),
            Name = vm.Username,
            Password = vm.Password.ToMd5String()
        });

        _messages.Success("初始化完成！");
        return RedirectToAction(nameof(Index));
    }
}