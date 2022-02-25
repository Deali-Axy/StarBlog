using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Photography;
using StarBlog.Web.ViewModels.Response;

namespace StarBlog.Web.Apis;

/// <summary>
/// 摄影
/// </summary>
[ApiController]
[Route("Api/[controller]")]
public class PhotoController : ControllerBase {
    private readonly PhotoService _photoService;

    public PhotoController(PhotoService photoService) {
        _photoService = photoService;
    }

    [HttpGet]
    public ApiResponsePaged<Photo> GetList(int page = 1, int pageSize = 10) {
        var paged = _photoService.GetPagedList(page, pageSize);
        return new ApiResponsePaged<Photo> {
            Pagination = paged.ToPaginationMetadata(),
            Data = paged.ToList()
        };
    }

    [HttpGet("{id}")]
    public ApiResponse<Photo> Get(string id) {
        var photo = _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound(Response);
        return new ApiResponse<Photo> { Data = photo };
    }

    [Authorize]
    [HttpPost]
    public ApiResponse<Photo> Add([FromForm] PhotoCreationDto dto, IFormFile file) {
        var photo = _photoService.Add(dto, file);

        return !ModelState.IsValid
            ? ApiResponse.BadRequest(Response, ModelState)
            : new ApiResponse<Photo>(photo);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public ApiResponse Delete(string id) {
        var photo = _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound(Response);
        var rows = _photoService.DeleteById(id);
        return rows > 0
            ? ApiResponse.Ok(Response, $"deleted {rows} rows.")
            : ApiResponse.Error(Response, "deleting failed.");
    }
}