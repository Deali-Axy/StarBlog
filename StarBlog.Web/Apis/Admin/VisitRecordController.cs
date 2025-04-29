using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services.VisitRecordServices;
using StarBlog.Web.Criteria;
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
    public async Task<ApiResponsePaged<VisitRecord>> GetList([FromQuery] VisitRecordParameters param) {
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
    public async Task<List<VisitRecord>> GetAll([FromQuery] VisitRecordParameters p) {
        return await _service.GetAll(p);
    }

    /// <summary>
    /// 总览数据
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<ApiResponse> Overview([FromQuery] VisitRecordParameters p) {
        return ApiResponse.Ok(await _service.Overview(p));
    }

    /// <summary>
    /// 趋势数据
    /// </summary>
    /// <param name="days">查看最近几天的数据，默认7天</param>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<ApiResponse> Trend([FromQuery] VisitRecordParameters p, int days = 7) {
        return ApiResponse.Ok(await _service.GetDailyTrend(p, days));
    }

    /// <summary>
    /// 获取地理信息筛选参数
    /// </summary>
    /// <param name="param">可选 country, province, city</param>
    [HttpGet("[action]")]
    public async Task<ApiResponse> GetGeoFilterParams([FromQuery] VisitRecordParameters p, string param = "country") {
        var r = await _service.GetGeoFilterParams(p, param);
        return ApiResponse.Ok(r);
    }

    /// <summary>
    /// 获取 UserAgent 筛选参数
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> GetUserAgentFilterParams([FromQuery] VisitRecordParameters p) {
        var r = await _service.GetUserAgentFilterParams(p);
        return ApiResponse.Ok(r);
    }

    /// <summary>
    /// 地理位置分布统计
    /// </summary>
    [HttpGet("GeoDistribution")]
    public async Task<ApiResponse> GeoDistribution([FromQuery] VisitRecordParameters p) {
        return ApiResponse.Ok(await _service.GetGeoDistribution(p));
    }

    /// <summary>
    /// 获取来源域名分析
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> ReferrerDomains([FromQuery] VisitRecordParameters p, int top = 10) {
        return ApiResponse.Ok(await _service.GetReferrerDomains(p, top));
    }

    /// <summary>
    /// 获取环比增长数据
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> GrowthRate([FromQuery] VisitRecordParameters p, int days = 7) {
        return ApiResponse.Ok(await _service.GetGrowthRate(p, days));
    }

    /// <summary>
    /// 获取跳出率统计
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> BounceRate([FromQuery] VisitRecordParameters p) {
        return ApiResponse.Ok(await _service.GetBounceRate(p));
    }

    /// <summary>
    /// 获取首次访问统计
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> FirstVisit([FromQuery] VisitRecordParameters p, [FromQuery] int days = 7) {
        return ApiResponse.Ok(await _service.GetFirstVisitStats(p, days));
    }

    /// <summary>
    /// 获取技术分布统计
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> TechDistribution([FromQuery] VisitRecordParameters p) {
        return ApiResponse.Ok(await _service.GetTechDistribution(p));
    }

    /// <summary>
    /// 获取慢请求排行
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> SlowRequests([FromQuery] VisitRecordParameters p, int top = 10) {
        return ApiResponse.Ok(await _service.GetSlowRequests(p, top));
    }

    /// <summary>
    /// 获取转化行为分析
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> Conversion([FromQuery] VisitRecordParameters p, string targetPath) {
        return ApiResponse.Ok(await _service.GetConversionStats(p, targetPath));
    }

    /// <summary>
    /// 获取小时级趋势
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> HourlyTrend([FromQuery] VisitRecordParameters p) {
        return ApiResponse.Ok(await _service.GetHourlyTrend(p));
    }

    /// <summary>
    /// 获取PV/UV统计
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> PvUv([FromQuery] VisitRecordParameters p) {
        return ApiResponse.Ok(await _service.GetPvUv(p));
    }

    /// <summary>
    /// 获取响应时间统计
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> ResponseTimeStats([FromQuery] VisitRecordParameters p) {
        return ApiResponse.Ok(await _service.GetResponseTimeStats(p));
    }

    /// <summary>
    /// 获取Top N访问路径
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> TopPaths([FromQuery] VisitRecordParameters p, int top = 10) {
        return ApiResponse.Ok(await _service.GetTopPaths(p, top));
    }
}