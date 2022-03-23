using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;
using StarBlog.Web.ViewModels.Blog;
using StarBlog.Web.ViewModels.QueryFilters;
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
    public ApiResponsePaged<Post> GetList([FromQuery] PostQueryParameters param) {
        var pagedList = _postService.GetPagedList(param);
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

    // todo 这个添加文章的参数是不对的，得换成 DTO
    [HttpPost]
    public ApiResponse<Post> Add(PostCreationDto dto,
        [FromServices] CategoryService categoryService) {
        var post = _mapper.Map<Post>(dto);
        var category = categoryService.GetById(dto.CategoryId);
        if (category == null) return ApiResponse.BadRequest($"分类 {dto.CategoryId} 不存在！");

        post.Id = Guid.NewGuid().ToString();
        post.CreationTime = DateTime.Now;
        post.LastUpdateTime = DateTime.Now;

        var categories = new List<Category> { category };
        var parent = category.Parent;
        while (parent != null) {
            categories.Add(parent);
            parent = parent.Parent;
        }

        categories.Reverse();
        post.Categories = string.Join(",", categories.Select(a => a.Id));

        return new ApiResponse<Post>(_postService.InsertOrUpdate(post));
    }

    [HttpPut("{id}")]
    public ApiResponse<Post> Update(string id, PostUpdateDto dto) {
        var post = _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");

        // mapper.Map(source) 得到一个全新的对象
        // mapper.Map(source, dest) 在 source 对象的基础上修改
        post = _mapper.Map(dto, post);
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