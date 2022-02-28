using FreeSql;
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
    private readonly IBaseRepository<Post> _postRepo;
    private readonly PostService _postService;

    public BlogPostController(IBaseRepository<Post> postRepo, PostService postService) {
        _postRepo = postRepo;
        _postService = postService;
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
        var post = _postRepo.Where(a => a.Id == id).First();
        if (post == null) return ApiResponse.NotFound();
        return new ApiResponse<Post> {Data = post};
    }
}