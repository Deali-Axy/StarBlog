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

    [HttpGet("All")]
    public ApiResponse<List<VisitRecord>> GetAll() {
        return new ApiResponse<List<VisitRecord>>(_service.GetAll());
    }

    [HttpGet("{id:int}")]
    public ApiResponse<VisitRecord> GetById(int id) {
        var item = _service.GetById(id);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<VisitRecord>(item);
    }
}