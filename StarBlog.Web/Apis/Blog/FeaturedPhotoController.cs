using CodeLab.Share.ViewModels.Response;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;

namespace StarBlog.Web.Apis.Blog;

/// <summary>
/// 推荐图片
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = "blog")]
public class FeaturedPhotoController : ControllerBase {
    private readonly PhotoService _photoService;
    private readonly IBaseRepository<FeaturedPhoto> _fpRepo;

    public FeaturedPhotoController(PhotoService photoService, IBaseRepository<FeaturedPhoto> fpRepo) {
        _photoService = photoService;
        _fpRepo = fpRepo;
    }

    [AllowAnonymous]
    [HttpGet]
    public ApiResponse<List<FeaturedPhoto>> GetList() {
        return new ApiResponse<List<FeaturedPhoto>>(_fpRepo.Select
            .Include(a => a.Photo).ToList());
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public ApiResponse<FeaturedPhoto> Get(int id) {
        var item = _fpRepo.Where(a => a.Id == id).First();
        return item == null
            ? ApiResponse.NotFound($"推荐图片记录 {id} 不存在")
            : new ApiResponse<FeaturedPhoto>(item);
    }

    [HttpPost]
    public ApiResponse<FeaturedPhoto> Add(string photoId) {
        var photo = _photoService.GetById(photoId);
        return photo == null
            ? ApiResponse.NotFound($"图片 {photoId} 不存在")
            : new ApiResponse<FeaturedPhoto>(_photoService.AddFeaturedPhoto(photo));
    }

    [HttpDelete("{id:int}")]
    public ApiResponse Delete(int id) {
        var item = _fpRepo.Where(a => a.Id == id).First();
        if (item == null) return ApiResponse.NotFound($"推荐图片记录 {id} 不存在");
        var rows = _fpRepo.Delete(item);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }
}