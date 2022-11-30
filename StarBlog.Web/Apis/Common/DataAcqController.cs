﻿using Microsoft.AspNetCore.Mvc;
using StarBlog.Web.Services;

namespace StarBlog.Web.Apis.Common;

/// <summary>
/// DataAcq 的一些接口整合
/// </summary>
[ApiController]
[Route("Api/[controller]/[action]")]
[ApiExplorerSettings(GroupName = "common")]
public class DataAcqController : ControllerBase {
    private readonly CrawlService _crawlService;

    public DataAcqController(CrawlService crawlService) {
        _crawlService = crawlService;
    }

    [HttpGet]
    public async Task<string> Poem() {
        return await _crawlService.GetPoem();
    }

    [HttpGet]
    public async Task<string> Hitokoto() {
        return await _crawlService.GetHitokoto();
    }
}