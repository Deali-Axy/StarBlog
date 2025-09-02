using Microsoft.AspNetCore.Mvc;
using StarBlog.Web.Contrib.SiteMessage;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Photography;

namespace StarBlog.Web.Controllers;

public class PhotographyController : Controller {
    private readonly PhotoService _photoService;
    private readonly MessageService _messages;

    public PhotographyController(PhotoService photoService, MessageService messages) {
        _photoService = photoService;
        _messages = messages;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10) {
        return View(new PhotographyViewModel {
            Photos = await _photoService.GetPagedList(page, pageSize)
        });
    }

    public async Task<IActionResult> Photo(string id) {
        var photo = await _photoService.GetById(id);
        if (photo == null) {
            _messages.Error($"图片 {id} 不存在！");
            return RedirectToAction(nameof(Index));
        }

        return View(photo);
    }

    public async Task<IActionResult> Next(string id) {
        var item = await _photoService.GetNext(id);
        if (item == null) {
            _messages.Warning("没有下一张图片了~");
            return RedirectToAction(nameof(Photo), new {id});
        }

        return RedirectToAction(nameof(Photo), new {id = item.Id});
    }

    public async Task<IActionResult> Previous(string id) {
        var item = await _photoService.GetPrevious(id);
        if (item == null) {
            _messages.Warning("没有上一张图片了~");
            return RedirectToAction(nameof(Photo), new {id});
        }

        return RedirectToAction(nameof(Photo), new {id = item.Id});
    }

    public async Task<IActionResult> RandomPhoto() {
        var item = await _photoService.GetRandomPhoto();
        if (item == null) {
            _messages.Error("当前没有图片，请先上传！");
            return RedirectToAction("Index", "Home");
        }

        _messages.Info($"随机推荐了图片 <b>{item.Title}</b> 给你~" +
                       $"<span class='ps-3'><a href=\"{Url.Action(nameof(RandomPhoto))}\">再来一次</a></span>");
        return RedirectToAction(nameof(Photo), new {id = item.Id});
    }
}