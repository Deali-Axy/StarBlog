using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;

namespace StarBlog.Web.Controllers;

public class PhotographyController : Controller {
    private readonly PhotoService _photoService;

    public PhotographyController(PhotoService photoService) {
        _photoService = photoService;
    }

    public IActionResult Index(int page = 1, int pageSize = 10) {
        return View(new PhotographyViewModel {
            Photos = _photoService.GetPagedList(page, pageSize)
        });
    }
}