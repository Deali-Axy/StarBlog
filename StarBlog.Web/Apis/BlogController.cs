using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;
using StarBlog.Web.ViewModels.Response;

namespace StarBlog.Web.Apis;

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
    [HttpGet("top")]
    public ApiResponse<Post> GetTopOnePost() {
        return new ApiResponse<Post> { Data = _blogService.GetTopOnePost() };
    }

    /// <summary>
    /// 获取推荐博客row，一行最多两个博客
    /// </summary>
    /// <returns></returns>
    [HttpGet("featured")]
    public ApiResponse<List<List<Post>>> GetFeaturedPostRows() {
        return new ApiResponse<List<List<Post>>> { Data = _blogService.GetFeaturedPostRows() };
    }
}