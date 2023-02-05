using AutoMapper;
using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Links;

namespace StarBlog.Web.Apis.Links;

/// <summary>
/// 友情链接
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
public class LinkController : ControllerBase {
    private readonly LinkService _service;
    private readonly IMapper _mapper;

    public LinkController(LinkService service, IMapper mapper) {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet]
    public List<Link> GetAll() {
        return _service.GetAll();
    }

    [HttpGet("{id:int}")]
    public ApiResponse<Link> Get(int id) {
        var item = _service.GetById(id);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<Link>(item);
    }

    [HttpPost]
    public ApiResponse<Link> Add(LinkCreationDto dto) {
        var link = _mapper.Map<Link>(dto);
        link = _service.AddOrUpdate(link);
        return new ApiResponse<Link>(link);
    }

    [HttpPut("{id:int}")]
    public ApiResponse<Link> Update(int id, LinkCreationDto dto) {
        var item = _service.GetById(id);
        if (item == null) return ApiResponse.NotFound();
        
        var link = _mapper.Map(dto, item);
        link = _service.AddOrUpdate(link);
        return new ApiResponse<Link>(link);
    }

    [HttpDelete("{id:int}")]
    public ApiResponse Delete(int id) {
        if (!_service.HasId(id)) return ApiResponse.NotFound();
        var rows = _service.DeleteById(id);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }
}