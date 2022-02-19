using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;

namespace StarBlog.Web.Apis;

/// <summary>
/// 博客
/// </summary>
[ApiController]
[Route("Api/[controller]")]
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
    public ActionResult<Post?> GetTopOnePost() {
        return _blogService.GetTopOnePost();
    }

    /// <summary>
    /// 获取推荐博客row，一行最多两个博客
    /// </summary>
    /// <returns></returns>
    [HttpGet("featured")]
    public ActionResult<List<List<Post>>> GetFeaturedPosts() {
        return _blogService.GetFeaturedPosts();
    }
}