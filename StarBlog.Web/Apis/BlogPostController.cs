using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;
using StarBlog.Web.ViewModels.Blog;
using StarBlog.Web.ViewModels.Response;
using X.PagedList;

namespace StarBlog.Web.Apis;

/// <summary>
/// 文章
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = "blog")]
public class BlogPostController : ControllerBase {
    private readonly IMapper _mapper;
    private readonly PostService _postService;
    private readonly BlogService _blogService;

    public BlogPostController(PostService postService, BlogService blogService, IMapper mapper) {
        _postService = postService;
        _blogService = blogService;
        _mapper = mapper;
    }

    [AllowAnonymous]
    [HttpGet]
    public ApiResponsePaged<Post> GetList(int categoryId = 0, int page = 1, int pageSize = 10) {
        var pagedList = _postService.GetPagedList(categoryId, page, pageSize);
        return new ApiResponsePaged<Post> {
            Message = "Get posts list",
            Data = pagedList.ToList(),
            Pagination = pagedList.ToPaginationMetadata()
        };
    }

    [AllowAnonymous]
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

    [HttpPost]
    public ApiResponse<Post> Add(Post post) {
        return new ApiResponse<Post>(_postService.InsertOrUpdate(post));
    }

    [HttpPut("{id}")]
    public ApiResponse<Post> Update(string id, PostUpdateDto dto) {
        var post = _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");

        post = _mapper.Map<Post>(dto);
        post.LastUpdateTime = DateTime.Now;
        return new ApiResponse<Post>(_postService.InsertOrUpdate(post));
    }

    /// <summary>
    /// 上传图片
    /// </summary>
    /// <param name="id"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public ApiResponse UploadImage(string id, IFormFile file) {
        var post = _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");
        var imgUrl = _postService.UploadImage(post, file);
        return ApiResponse.Ok(new { imgUrl });
    }

    /// <summary>
    /// 获取文章里的图片
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/[action]")]
    public ApiResponse<List<string>> Images(string id) {
        var post = _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");
        return new ApiResponse<List<string>>(_postService.GetImages(post));
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
        return new ApiResponse<TopPost> { Data = data, Message = $"ok. deleted {rows} old topPosts." };
    }
}