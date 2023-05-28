using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Contrib.SiteMessage;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.LinkExchange;

namespace StarBlog.Web.Controllers;

public class LinkExchangeController : Controller {
    private readonly ILogger<LinkExchangeController> _logger;
    private readonly LinkExchangeService _service;
    private readonly IMapper _mapper;
    private readonly MessageService _messages;

    public LinkExchangeController(ILogger<LinkExchangeController> logger, LinkExchangeService service, IMapper mapper,
        MessageService messages) {
        _logger = logger;
        _service = service;
        _mapper = mapper;
        _messages = messages;
    }

    [HttpGet]
    public IActionResult Add() {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(LinkExchangeAddViewModel vm) {
        if (!ModelState.IsValid) return View();

        if (await _service.HasUrl(vm.Url)) {
            _messages.Error("相同网址的友链申请已提交！");
            return View();
        }

        var item = _mapper.Map<LinkExchange>(vm);
        item = await _service.AddOrUpdate(item);

        // 发送邮件通知
        await _service.SendEmailOnAdd(item);

        _messages.Info("友链申请已提交，正在处理中，请及时关注邮件通知~");
        return RedirectToAction("Index", "Home");
    }
}