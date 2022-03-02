using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Contrib.SiteMessage;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;

namespace StarBlog.Web.Controllers;

public class PhotographyController : Controller {
    private readonly PhotoService _photoService;
    private readonly Messages _messages;

    public PhotographyController(PhotoService photoService, Messages messages) {
        _photoService = photoService;
        _messages = messages;
    }

    public IActionResult Index(int page = 1, int pageSize = 10) {
        return View(new PhotographyViewModel {
            Photos = _photoService.GetPagedList(page, pageSize)
        });
    }

    public IActionResult Photo(string id) {
        return View(_photoService.GetById(id));
    }

    public IActionResult RandomPhoto() {
        var items = _photoService.GetAll();
        var rndItem = items[new Random().Next(items.Count)];
        _messages.Info($"随机推荐了图片 <b>{rndItem.Title}</b> 给你~");
        return RedirectToAction(nameof(Photo), new {id = rndItem.Id});
    }
}