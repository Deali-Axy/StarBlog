using AutoMapper;
using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Api.Extensions;
using StarBlog.Application.Services;
using StarBlog.Application.ViewModels.Links;

namespace StarBlog.Api.Apis.Links;

/// <summary>
/// 友情链接
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[Route("Api/Admin/Links")]
[ApiExplorerSettings(GroupName = ApiGroups.Link)]
public class LinkController : ControllerBase {
    private readonly LinkService _service;
    private readonly IMapper _mapper;

    public LinkController(LinkService service, IMapper mapper) {
        _service = service;
        _mapper = mapper;
    }

    /// <summary>
    /// 前台展示用：仅返回可见友情链接（匿名可访问）
    /// </summary>
    [AllowAnonymous]
    [HttpGet("Public")]
    public async Task<ApiResponse<List<Link>>> GetPublic() {
        var data = await _service.GetAll(true);
        return new ApiResponse<List<Link>>(data);
    }

    [HttpGet]
    public async Task<ApiResponse<List<Link>>> GetAll() {
        var data = await _service.GetAll(false);
        return new ApiResponse<List<Link>>(data);
    }

    [HttpGet("{id:int}")]
    public async Task<ApiResponse<Link>> Get(int id) {
        var item = await _service.GetById(id);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<Link>(item);
    }

    [HttpPost]
    public async Task<ApiResponse<Link>> Add(LinkCreationDto dto) {
        var link = _mapper.Map<Link>(dto);
        link = await _service.AddOrUpdate(link);
        return new ApiResponse<Link>(link);
    }

    [HttpPut("{id:int}")]
    public async Task<ApiResponse<Link>> Update(int id, LinkCreationDto dto) {
        var item = await _service.GetById(id);
        if (item == null) return ApiResponse.NotFound();

        var link = _mapper.Map(dto, item);
        link = await _service.AddOrUpdate(link);
        return new ApiResponse<Link>(link);
    }

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id) {
        if (!await _service.HasId(id)) return ApiResponse.NotFound();
        var rows = await _service.DeleteById(id);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }
}
