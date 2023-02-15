using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.QueryFilters;

namespace StarBlog.Web.Apis.Admin;

/// <summary>
/// 访问记录
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Admin)]
public class VisitRecordController : ControllerBase {
    private readonly VisitRecordService _service;

    public VisitRecordController(VisitRecordService service) {
        _service = service;
    }

    [HttpGet]
    public async Task<ApiResponsePaged<VisitRecord>> GetList([FromQuery] VisitRecordQueryParameters param) {
        var pagedList = await _service.GetPagedList(param);
        return new ApiResponsePaged<VisitRecord>(pagedList);
    }

    [HttpGet("{id:int}")]
    public async Task<ApiResponse<VisitRecord>> GetById(int id) {
        var item = await _service.GetById(id);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<VisitRecord>(item);
    }

    /// <summary>
    /// 获取全部访问记录
    /// </summary>
    /// <returns></returns>
    [HttpGet("All")]
    public async Task<List<VisitRecord>> GetAll() {
        return await _service.GetAll();
    }

    /// <summary>
    /// 总览数据
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<ApiResponse> Overview() {
        return ApiResponse.Ok(await _service.Overview());
    }

    /// <summary>
    /// 趋势数据
    /// </summary>
    /// <param name="days">查看最近几天的数据，默认7天</param>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<ApiResponse> Trend(int days = 7) {
        return ApiResponse.Ok(await _service.Trend(days));
    }

    /// <summary>
    /// 统计接口
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<ApiResponse> Stats(int year, int month, int day) {
        var date = new DateTime(year, month, day);
        return ApiResponse.Ok(await _service.Stats(date));
    }
}