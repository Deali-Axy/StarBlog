using AutoMapper;
using CodeLab.Share.Extensions;
using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Share.Utils;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;
using StarBlog.Web.ViewModels.Blog;
using StarBlog.Web.ViewModels.QueryFilters;
using X.PagedList;

namespace StarBlog.Web.Apis.Blog;

/// <summary>
/// 文章
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
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
    public async Task<ApiResponsePaged<Post>> GetList([FromQuery] PostQueryParameters param) {
        var pagedList = await _postService.GetPagedList(param);
        return new ApiResponsePaged<Post> {
            Message = "Get posts list",
            Data = pagedList.ToList(),
            Pagination = pagedList.ToPaginationMetadata()
        };
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ApiResponse<Post>> Get(string id) {
        var post = await _postService.GetById(id);
        return post == null ? ApiResponse.NotFound() : new ApiResponse<Post>(post);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse> Delete(string id) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");
        var rows = _postService.Delete(id);
        return ApiResponse.Ok($"删除了 {rows} 篇博客");
    }

    // todo 发表文章需要同时设置已发布状态
    [HttpPost]
    public async Task<ApiResponse<Post>> Add(PostCreationDto dto,
        [FromServices] CategoryService categoryService) {
        var post = _mapper.Map<Post>(dto);
        var category = await categoryService.GetById(dto.CategoryId);
        if (category == null) return ApiResponse.BadRequest($"分类 {dto.CategoryId} 不存在！");

        post.Id = GuidUtils.GuidTo16String();
        post.CreationTime = DateTime.Now;
        post.LastUpdateTime = DateTime.Now;
        post.IsPublish = true;

        // 获取分类的层级结构
        post.Categories = categoryService.GetCategoryBreadcrumb(category);

        return new ApiResponse<Post>(await _postService.InsertOrUpdateAsync(post));
    }

    [HttpPut("{id}")]
    public async Task<ApiResponse<Post>> Update(string id, PostUpdateDto dto) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");

        // mapper.Map(source) 得到一个全新的对象
        // mapper.Map(source, dest) 在 dest 对象的基础上修改
        post = _mapper.Map(dto, post);
        post.LastUpdateTime = DateTime.Now;
        return new ApiResponse<Post>(await _postService.InsertOrUpdateAsync(post));
    }

    /// <summary>
    /// 上传图片
    /// </summary>
    /// <param name="id"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse> UploadImage(string id, IFormFile file) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");
        var imgUrl = await _postService.UploadImage(post, file);
        return ApiResponse.Ok(new {
            imgUrl,
            imgName = Path.GetFileNameWithoutExtension(imgUrl)
        });
    }

    /// <summary>
    /// 获取文章里的图片
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/[action]")]
    public async Task<ApiResponse<List<string>>> Images(string id) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");
        return new ApiResponse<List<string>>(_postService.GetImages(post));
    }

    /// <summary>
    /// 设置为推荐博客
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse<FeaturedPost>> SetFeatured(string id) {
        var post = await _postService.GetById(id);
        return post == null
            ? ApiResponse.NotFound()
            : new ApiResponse<FeaturedPost>(await _blogService.AddFeaturedPost(post));
    }

    /// <summary>
    /// 取消推荐博客
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse> CancelFeatured(string id) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");
        var rows = await _blogService.DeleteFeaturedPost(post);
        return ApiResponse.Ok($"delete {rows} rows.");
    }

    /// <summary>
    /// 设置置顶（只能有一篇置顶）
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse<TopPost>> SetTop(string id) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");
        var (data, rows) = await _blogService.SetTopPost(post);
        return new ApiResponse<TopPost> {Data = data, Message = $"ok. deleted {rows} old topPosts."};
    }
}