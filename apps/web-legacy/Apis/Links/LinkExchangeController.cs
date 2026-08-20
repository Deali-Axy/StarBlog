using AutoMapper;
using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.LinkExchange;

namespace StarBlog.Web.Apis.Links;

/// <summary>
/// 友情链接申请
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Link)]
public class LinkExchangeController : ControllerBase {
    private readonly ILogger<LinkExchangeController> _logger;
    private readonly LinkExchangeService _service;
    private readonly IMapper _mapper;

    public LinkExchangeController(LinkExchangeService service, ILogger<LinkExchangeController> logger, IMapper mapper) {
        _service = service;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<List<LinkExchange>> GetAll() {
        return await _service.GetAll();
    }

    [HttpGet("{id:int}")]
    public async Task<ApiResponse<LinkExchange>> Get(int id) {
        var item = await _service.GetById(id);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<LinkExchange>(item);
    }

    [HttpPost("{id:int}/[action]")]
    public async Task<ApiResponse> Accept(int id, [FromBody] LinkExchangeVerityDto dto) {
        if (!await _service.HasId(id)) return ApiResponse.NotFound();
        await _service.SetVerifyStatus(id, true, dto.Reason);
        return ApiResponse.Ok();
    }

    [HttpPost("{id:int}/[action]")]
    public async Task<ApiResponse> Reject(int id, [FromBody] LinkExchangeVerityDto dto) {
        if (!await _service.HasId(id)) return ApiResponse.NotFound();
        await _service.SetVerifyStatus(id, false, dto.Reason);
        return ApiResponse.Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id) {
        if (!await _service.HasId(id)) return ApiResponse.NotFound();
        var rows = await _service.DeleteById(id);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }
}