using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.QueryFilters;
using StarBlog.Web.ViewModels.Response;

namespace StarBlog.Web.Apis;

[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = "admin")]
public class VisitRecordController : ControllerBase {
    private readonly VisitRecordService _service;

    public VisitRecordController(VisitRecordService service) {
        _service = service;
    }

    [HttpGet]
    public ApiResponsePaged<VisitRecord> GetList([FromQuery] VisitRecordQueryParameters param) {
        var pagedList = _service.GetPagedList(param);
        return new ApiResponsePaged<VisitRecord>(pagedList);
    }

    [HttpGet("{id:int}")]
    public ApiResponse<VisitRecord> GetById(int id) {
        var item = _service.GetById(id);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<VisitRecord>(item);
    }

    /// <summary>
    /// 获取全部访问记录
    /// </summary>
    /// <returns></returns>
    [HttpGet("All")]
    public ApiResponse<List<VisitRecord>> GetAll() {
        return new ApiResponse<List<VisitRecord>>(_service.GetAll());
    }

    /// <summary>
    /// 总览数据
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public ApiResponse Overview() {
        return ApiResponse.Ok(_service.Overview());
    }

    /// <summary>
    /// 趋势数据
    /// </summary>
    /// <param name="days">查看最近几天的数据，默认7天</param>
    /// <returns></returns>
    [HttpGet("[action]")]
    public ApiResponse Trend(int days = 7) {
        return ApiResponse.Ok(_service.Trend(days));
    }

    /// <summary>
    /// 统计接口
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public ApiResponse Stats(int year, int month, int day) {
        var date = new DateTime(year, month, day);
        return ApiResponse.Ok(_service.Stats(date));
    }
}