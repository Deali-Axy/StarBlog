using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;
using StarBlog.Web.ViewModels.Blog;
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
}