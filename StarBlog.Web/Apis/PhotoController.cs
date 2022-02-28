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
[ApiExplorerSettings(GroupName = "blog")]
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
        if (photo == null) return ApiResponse.NotFound();
        return new ApiResponse<Photo> { Data = photo };
    }

    [Authorize]
    [HttpPost]
    public ApiResponse<Photo> Add([FromForm] PhotoCreationDto dto, IFormFile file) {
        var photo = _photoService.Add(dto, file);

        return !ModelState.IsValid
            ? ApiResponse.BadRequest(ModelState)
            : new ApiResponse<Photo>(photo);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public ApiResponse Delete(string id) {
        var photo = _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound();
        var rows = _photoService.DeleteById(id);
        return rows > 0
            ? ApiResponse.Ok($"deleted {rows} rows.")
            : ApiResponse.Error(Response, "deleting failed.");
    }

    /// <summary>
    /// 重建图片库数据（重新扫描每张图片的大小等数据）
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpPost("[action]")]
    public ApiResponse ReBuildData() {
        return ApiResponse.Ok(new {
            Rows = _photoService.ReBuildData()
        }, "重建图片库数据");
    }

    /// <summary>
    /// 批量导入图片
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpPost("[action]")]
    public ApiResponse<List<Photo>> BatchImport() {
        var result = _photoService.BatchImport();
        return new ApiResponse<List<Photo>> {
            Data = result,
            Message = $"成功导入{result.Count}张图片"
        };
    }
}