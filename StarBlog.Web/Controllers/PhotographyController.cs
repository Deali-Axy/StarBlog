﻿using FreeSql;
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
        var photo = _photoService.GetById(id);
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

    public IActionResult RandomPhoto() {
        var item = _photoService.GetRandomPhoto();
        if (item == null) {
            _messages.Error("当前没有图片，请先上传！");
            return RedirectToAction("Index", "Home");
        }

        _messages.Info($"随机推荐了图片 <b>{item.Title}</b> 给你~");
        return RedirectToAction(nameof(Photo), new {id = item.Id});
    }
}