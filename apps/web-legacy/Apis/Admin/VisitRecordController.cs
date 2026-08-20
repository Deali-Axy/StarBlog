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
    private readonly VisitRecordAnalyticsService _service;

    public VisitRecordController(VisitRecordAnalyticsService service) {
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
        return item == null ? ApiResponse.NotFound() : item;
    }

    /// <summary>
    /// 获取全部访问记录
    /// </summary>
    /// <returns></returns>
    [HttpGet("All")]
    public async Task<ApiResponse<List<VisitRecord>>> GetAll([FromQuery] VisitRecordParameters p) {
        return await _service.GetAll(p);
    }

    /// <summary>
    /// 总览数据
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<ApiResponse<VisitOverview>> Overview([FromQuery] VisitRecordParameters p) {
        return await _service.Overview(p);
    }

    /// <summary>
    /// 趋势数据
    /// </summary>
    /// <param name="days">查看最近几天的数据，默认7天</param>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<ApiResponse<List<DailyTrend>>> DailyTrend([FromQuery] VisitRecordParameters p, int days = 7) {
        return await _service.GetDailyTrend(p, days);
    }
    
    /// <summary>
    /// 获取小时级趋势
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<List<HourlyTrend>>> HourlyTrend([FromQuery] VisitRecordParameters p) {
        return await _service.GetHourlyTrend(p);
    }

    /// <summary>
    /// 获取地理信息筛选参数
    /// </summary>
    /// <param name="param">可选 country, province, city</param>
    [HttpGet("[action]")]
    public async Task<ApiResponse<List<string?>>> GeoFilterParams([FromQuery] VisitRecordParameters p, string param = "country") {
        return await _service.GetGeoFilterParams(p, param);
    }

    /// <summary>
    /// 获取 UserAgent 筛选参数
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<UserAgentFilterParams>> UserAgentFilterParams([FromQuery] VisitRecordParameters p) {
        return await _service.GetUserAgentFilterParams(p);
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
    public async Task<ApiResponse<List<ReferrerDomain>>> ReferrerDomains([FromQuery] VisitRecordParameters p, int top = 10) {
        return await _service.GetReferrerDomains(p, top);
    }

    /// <summary>
    /// 获取环比增长数据
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<GrowthRate>> GrowthRate([FromQuery] VisitRecordParameters p, int days = 7) {
        return await _service.GetGrowthRate(p, days);
    }

    /// <summary>
    /// 获取跳出率统计
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<BounceRateStats>> BounceRate([FromQuery] VisitRecordParameters p) {
        return await _service.GetBounceRate(p);
    }

    /// <summary>
    /// 获取首次访问统计
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<FirstVisitStats>> FirstVisit([FromQuery] VisitRecordParameters p, [FromQuery] int days = 7) {
        return await _service.GetFirstVisitStats(p, days);
    }

    /// <summary>
    /// 获取技术分布统计
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<TechDistribution>> TechDistribution([FromQuery] VisitRecordParameters p) {
        return await _service.GetTechDistribution(p);
    }

    /// <summary>
    /// 获取慢请求排行
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<List<SlowRequest>>> SlowRequests([FromQuery] VisitRecordParameters p, int top = 10) {
        return await _service.GetSlowRequests(p, top);
    }

    /// <summary>
    /// 获取转化行为分析
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<ConversionStats>> Conversion([FromQuery] VisitRecordParameters p, string targetPath) {
        return await _service.GetConversionStats(p, targetPath);
    }

    /// <summary>
    /// 获取PV/UV统计
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<PvUv>> PvUv([FromQuery] VisitRecordParameters p) {
        return await _service.GetPvUv(p);
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
    public async Task<ApiResponse<List<TopPath>>> TopPaths([FromQuery] VisitRecordParameters p, int top = 10) {
        return await _service.GetTopPaths(p, top);
    }
}
