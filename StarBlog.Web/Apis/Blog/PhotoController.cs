using CodeLab.Share.Extensions;
using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Photography;

namespace StarBlog.Web.Apis.Blog;

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
        return photo == null
            ? ApiResponse.NotFound($"图片 {id} 不存在")
            : new ApiResponse<Photo> { Data = photo };
    }

    [HttpGet("{id}/Thumb")]
    public async Task<IActionResult> GetThumb(string id, [FromQuery] int width = 300) {
        var data = await _photoService.GetThumb(id, width);
        return new FileContentResult(data, "image/jpeg");
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
        if (photo == null) return ApiResponse.NotFound($"图片 {id} 不存在");
        var rows = _photoService.DeleteById(id);
        return rows > 0
            ? ApiResponse.Ok($"deleted {rows} rows.")
            : ApiResponse.Error("deleting failed.");
    }

    /// <summary>
    /// 设置为推荐图片
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public ApiResponse<FeaturedPhoto> SetFeatured(string id) {
        var photo = _photoService.GetById(id);
        return photo == null
            ? ApiResponse.NotFound($"图片 {id} 不存在")
            : new ApiResponse<FeaturedPhoto>(_photoService.AddFeaturedPhoto(photo));
    }

    /// <summary>
    /// 取消推荐
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public ApiResponse CancelFeatured(string id) {
        var photo = _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound($"图片 {id} 不存在");
        var rows = _photoService.DeleteFeaturedPhoto(photo);
        return ApiResponse.Ok($"deleted {rows} rows.");
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