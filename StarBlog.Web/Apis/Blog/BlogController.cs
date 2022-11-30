using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;
using StarBlog.Web.ViewModels.Blog;
using StarBlog.Web.ViewModels.Response;

namespace StarBlog.Web.Apis.Blog;

/// <summary>
/// 博客
/// </summary>
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = "blog")]
public class BlogController : ControllerBase {
    private readonly BlogService _blogService;


    public BlogController(BlogService blogService) {
        _blogService = blogService;
    }

    /// <summary>
    /// 获取置顶博客
    /// </summary>
    /// <returns></returns>
    [HttpGet("Top")]
    public ApiResponse<Post> GetTopOnePost() {
        return new ApiResponse<Post> {Data = _blogService.GetTopOnePost()};
    }

    /// <summary>
    /// 获取推荐博客
    /// </summary>
    /// <returns></returns>
    [HttpGet("Featured")]
    public ApiResponse<List<Post>> GetFeaturedPostRows() {
        return new ApiResponse<List<Post>>(_blogService.GetFeaturedPosts());
    }

    /// <summary>
    /// 博客信息概况
    /// </summary>
    /// <returns></returns>
    // [Authorize]
    [HttpGet("[action]")]
    public ApiResponse<BlogOverview> Overview() {
        return new ApiResponse<BlogOverview>(_blogService.Overview());
    }

    /// <summary>
    /// 博客文章状态列表
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public ApiResponse GetStatusList() {
        return ApiResponse.Ok(_blogService.GetStatusList());
    }

    /// <summary>
    /// 上传博客压缩包 + 导入
    /// </summary>
    /// <returns></returns>
    [HttpPost("[action]")]
    public async Task<ApiResponse<Post>> Upload([FromForm] PostCreationDto dto, IFormFile file,
        [FromServices] CategoryService categoryService
    ) {
        if (!file.FileName.EndsWith(".zip")) {
            return ApiResponse.BadRequest("只能上传zip格式的文件哦~");
        }

        var category = categoryService.GetById(dto.CategoryId);
        if (category == null) return ApiResponse.BadRequest($"分类 {dto.CategoryId} 不存在！");

        try {
            return new ApiResponse<Post>(await _blogService.Upload(dto, file));
        }
        catch (Exception ex) {
            return ApiResponse.Error($"解压文件出错：{ex.Message}");
        }
    }
}