using CodeLab.Share.ViewModels.Response;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;
using StarBlog.Web.ViewModels.Blog;

namespace StarBlog.Web.Apis.Blog;

/// <summary>
/// 博客
/// </summary>
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
public class BlogController : ControllerBase {
    private readonly ILogger<BlogController> _logger;
    private readonly BlogService _blogService;


    public BlogController(BlogService blogService, ILogger<BlogController> logger) {
        _blogService = blogService;
        _logger = logger;
    }

    /// <summary>
    /// 获取置顶博客
    /// </summary>
    /// <returns></returns>
    [HttpGet("Top")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<Post?>))]
    public async Task<Post?> GetTopOnePost() {
        return await _blogService.GetTopOnePost();
    }

    /// <summary>
    /// 获取推荐博客
    /// </summary>
    /// <returns></returns>
    [HttpGet("Featured")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<Post>>))]
    public async Task<List<Post>> GetFeaturedPosts() {
        return await _blogService.GetFeaturedPosts();
    }

    /// <summary>
    /// 博客信息概况
    /// </summary>
    /// <returns></returns>
    // [Authorize]
    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<BlogOverview>))]
    public async Task<BlogOverview> Overview() {
        return await _blogService.Overview();
    }

    /// <summary>
    /// 博客文章状态列表
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string?>))]
    public async Task<List<string?>> GetStatusList() {
        return await _blogService.GetStatusList();
    }

    /// <summary>
    /// 上传博客压缩包 + 导入
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpPost("[action]")]
    public async Task<ApiResponse<Post>> Upload([FromForm] PostCreationDto dto, IFormFile file,
        [FromServices] CategoryService categoryService
    ) {
        if (!file.FileName.EndsWith(".zip")) {
            return ApiResponse.BadRequest("只能上传zip格式的文件哦~");
        }

        var category = await categoryService.GetById(dto.CategoryId);
        if (category == null) return ApiResponse.BadRequest($"分类 {dto.CategoryId} 不存在！");

        try {
            return new ApiResponse<Post>(await _blogService.Upload(dto, file));
        }
        catch (Exception ex) {
            _logger.LogError(ex, "解压文件出错：{message}", ex.Message);
            return ApiResponse.Error($"解压文件出错：{ex.Message}");
        }
    }
}