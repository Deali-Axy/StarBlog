using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Response;

namespace StarBlog.Web.Apis;

/// <summary>
/// 推荐图片
/// </summary>
[ApiController]
[Route("Api/[controller]")]
public class FeaturedPhotoController : ControllerBase {
    private readonly PhotoService _photoService;

    public FeaturedPhotoController(PhotoService photoService) {
        _photoService = photoService;
    }

    [HttpGet]
    public ApiResponse<List<Photo>> GetList() {
        return new ApiResponse<List<Photo>>(_photoService.GetFeaturedPhotos());
    }
}