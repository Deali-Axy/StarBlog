using CodeLab.Share.ViewModels.Response;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Api.Extensions;
using StarBlog.Application.Services;
using StarBlog.Application.ViewModels;
using StarBlog.Application.ViewModels.Blog;

namespace StarBlog.Api.Apis.Blog;

/// <summary>
/// 博客
/// </summary>
[ApiController]
[Route("Api/[controller]")]
[Route("Api/Admin/Blog")]
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PostDto?>))]
    public async Task<PostDto?> GetTopOnePost() {
        var post = await _blogService.GetTopOnePost();
        return post == null ? null : PostDto.From(post);
    }

    /// <summary>
    /// 获取推荐博客
    /// </summary>
    /// <returns></returns>
    [HttpGet("Featured")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<PostDto>>))]
    public async Task<List<PostDto>> GetFeaturedPosts() {
        var posts = await _blogService.GetFeaturedPosts();
        return posts.Select(PostDto.From).ToList();
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
    public async Task<ApiResponse<PostDto>> Upload([FromForm] PostCreationDto dto, IFormFile file,
        [FromServices] CategoryService categoryService
    ) {
        if (!file.FileName.EndsWith(".zip")) {
            return ApiResponse.BadRequest("只能上传zip格式的文件哦~");
        }

        var category = await categoryService.GetById(dto.CategoryId);
        if (category == null) return ApiResponse.BadRequest($"分类 {dto.CategoryId} 不存在！");

        try {
            await using var stream = file.OpenReadStream();
            var post = await _blogService.Upload(dto, stream);
            return new ApiResponse<PostDto>(PostDto.From(post));
        }
        catch (Exception ex) {
            _logger.LogError(ex, "解压文件出错：{message}", ex.Message);
            return ApiResponse.Error($"解压文件出错：{ex.Message}");
        }
    }
}
