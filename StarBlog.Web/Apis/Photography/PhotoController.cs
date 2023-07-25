using CodeLab.Share.Extensions;
using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Photography;

namespace StarBlog.Web.Apis.Photography;

/// <summary>
/// 摄影/图片
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Photo)]
public class PhotoController : ControllerBase {
    private readonly PhotoService _photoService;

    public PhotoController(PhotoService photoService) {
        _photoService = photoService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ApiResponsePaged<Photo>> GetList(int page = 1, int pageSize = 10) {
        var paged = await _photoService.GetPagedList(page, pageSize);
        return new ApiResponsePaged<Photo> {
            Pagination = paged.ToPaginationMetadata(),
            Data = paged.ToList()
        };
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ApiResponse<Photo>> Get(string id) {
        var photo = await _photoService.GetById(id);
        return photo == null
            ? ApiResponse.NotFound($"图片 {id} 不存在")
            : new ApiResponse<Photo> { Data = photo };
    }

    /// <summary>
    /// 获取指定宽度的缩略图
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}/Thumb")]
    public async Task<IActionResult> GetThumb(string id, [FromQuery] int width = 300) {
        var data = await _photoService.GetThumb(id, width);
        return new FileContentResult(data, "image/jpeg");
    }

    [HttpPost]
    public async Task<ApiResponse<Photo>> Add([FromForm] PhotoCreationDto dto, IFormFile file) {
        var photo = await _photoService.Add(dto, file);

        return !ModelState.IsValid
            ? ApiResponse.BadRequest(ModelState)
            : new ApiResponse<Photo>(photo);
    }

    [HttpPut("{id}")]
    public async Task<ApiResponse<Photo>> Update(string id, [FromBody] PhotoUpdateDto dto) {
        var photo = await _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound($"图片 {id} 不存在");
        dto.Id = id;
        return new ApiResponse<Photo>(await _photoService.Update(dto)) { Message = "修改照片信息成功。" };
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse> Delete(string id) {
        var photo = await _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound($"图片 {id} 不存在");
        var rows = await _photoService.DeleteById(id);
        return rows > 0
            ? ApiResponse.Ok($"deleted {rows} rows.")
            : ApiResponse.Error("deleting failed.");
    }

    /// <summary>
    /// 设置为推荐图片
    /// </summary>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse<FeaturedPhoto>> SetFeatured(string id) {
        var photo = await _photoService.GetById(id);
        return photo == null
            ? ApiResponse.NotFound($"图片 {id} 不存在")
            : new ApiResponse<FeaturedPhoto>(await _photoService.AddFeaturedPhoto(photo));
    }

    /// <summary>
    /// 取消推荐
    /// </summary>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse> CancelFeatured(string id) {
        var photo = await _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound($"图片 {id} 不存在");
        var rows = await _photoService.DeleteFeaturedPhoto(photo);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }

    /// <summary>
    /// 重建图片库数据（重新扫描每张图片的大小等数据）
    /// </summary>
    [HttpPost("[action]")]
    public async Task<ApiResponse> ReBuildData() {
        return ApiResponse.Ok(new {
            Rows = await _photoService.ReBuildData()
        }, "重建图片库数据");
    }

    /// <summary>
    /// 批量导入图片
    /// </summary>
    [HttpPost("[action]")]
    public async Task<ApiResponse<List<Photo>>> BatchImport() {
        var result = await _photoService.BatchImport();
        return new ApiResponse<List<Photo>> {
            Data = result,
            Message = $"成功导入{result.Count}张图片"
        };
    }
}