using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.QueryFilters;
using StarBlog.Web.ViewModels.VisitRecord;

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
    public async Task<ApiResponse> Stats([FromQuery] StatsDto dto) {
        var date = new DateTime(dto.Year, dto.Month, dto.Day);
        return ApiResponse.Ok(await _service.Stats(date));
    }

    /// <summary>
    /// 获取地理信息筛选参数
    /// </summary>
    /// <param name="param">可选 country, province, city</param>
    /// <param name="country"></param>
    /// <param name="province"></param>
    /// <param name="city"></param>
    [HttpGet("[action]")]
    public async Task<ApiResponse> GetGeoFilterParams(string param = "country", string? country = null, string? province = null, string? city = null) {
        var r = await _service.GetGeoFilterParams(param, country, province, city);
        return ApiResponse.Ok(r);
    }

    /// <summary>
    /// 获取 UserAgent 筛选参数
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> GetUserAgentFilterParams() {
        var r = await _service.GetUserAgentFilterParams();
        return ApiResponse.Ok(r);
    }

    /// <summary>
    /// 地理位置分布统计
    /// </summary>
    [HttpGet("GeoDistribution")]
    public async Task<ApiResponse> GeoDistribution() {
        return ApiResponse.Ok(await _service.GetGeoDistribution());
    }
}