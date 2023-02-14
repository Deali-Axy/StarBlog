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
    private readonly BlogService _blogService;


    public BlogController(BlogService blogService) {
        _blogService = blogService;
    }

    /// <summary>
    /// 获取置顶博客
    /// </summary>
    /// <returns></returns>
    [HttpGet("Top")]
    public async Task<Post?> GetTopOnePost() {
        return await _blogService.GetTopOnePost();
    }

    /// <summary>
    /// 获取推荐博客
    /// </summary>
    /// <returns></returns>
    [HttpGet("Featured")]
    public async Task<List<Post>> GetFeaturedPosts() {
        return await _blogService.GetFeaturedPosts();
    }

    /// <summary>
    /// 博客信息概况
    /// </summary>
    /// <returns></returns>
    // [Authorize]
    [HttpGet("[action]")]
    public async Task<BlogOverview> Overview() {
        return await _blogService.Overview();
    }

    /// <summary>
    /// 博客文章状态列表
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<List<string?>> GetStatusList() {
        return await _blogService.GetStatusList();
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