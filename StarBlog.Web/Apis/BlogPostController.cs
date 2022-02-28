using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;
using StarBlog.Web.ViewModels.Response;
using X.PagedList;

namespace StarBlog.Web.Apis;

/// <summary>
/// 文章
/// </summary>
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = "blog")]
public class BlogPostController : ControllerBase {
    private readonly PostService _postService;
    private readonly BlogService _blogService;

    public BlogPostController(PostService postService, BlogService blogService) {
        _postService = postService;
        _blogService = blogService;
    }

    [HttpGet]
    public ApiResponsePaged<Post> GetList(int categoryId = 0, int page = 1, int pageSize = 10) {
        var pagedList = _postService.GetPagedList(categoryId, page, pageSize);
        return new ApiResponsePaged<Post> {
            Message = "Get posts list",
            Data = pagedList.ToList(),
            Pagination = pagedList.ToPaginationMetadata()
        };
    }

    [HttpGet("{id}")]
    public ApiResponse<Post> Get(string id) {
        var post = _postService.GetById(id);
        return post == null ? ApiResponse.NotFound() : new ApiResponse<Post>(post);
    }

    [HttpDelete("{id}")]
    public ApiResponse Delete(string id) {
        var post = _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");
        var rows = _postService.Delete(id);
        return ApiResponse.Ok($"删除了 {rows} 篇博客");
    }

    /// <summary>
    /// 设置为推荐博客
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public ApiResponse<FeaturedPost> SetFeatured(string id) {
        var post = _postService.GetById(id);
        return post == null
            ? ApiResponse.NotFound()
            : new ApiResponse<FeaturedPost>(_blogService.AddFeaturedPost(post));
    }

    /// <summary>
    /// 取消推荐博客
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public ApiResponse CancelFeatured(string id) {
        var post = _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");
        var rows = _blogService.DeleteFeaturedPost(post);
        return ApiResponse.Ok($"delete {rows} rows.");
    }

    /// <summary>
    /// 设置置顶（只能有一篇置顶）
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public ApiResponse<TopPost> SetTop(string id) {
        var post = _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");
        var (data, rows) = _blogService.SetTopPost(post);
        return new ApiResponse<TopPost> {Data = data, Message = $"ok. deleted {rows} old topPosts."};
    }
}