using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;
using X.PagedList;

namespace StarBlog.Web.Apis;

/// <summary>
/// 文章
/// </summary>
[ApiController]
[Route("Api/[controller]")]
public class BlogPostController : ControllerBase {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly PostService _postService;

    public BlogPostController(IBaseRepository<Post> postRepo, PostService postService) {
        _postRepo = postRepo;
        _postService = postService;
    }

    [HttpGet]
    public ActionResult<PagedResponse<Post>> GetList(int categoryId = 0, int page = 1, int pageSize = 10) {
        var pagedList = _postService.GetPagedList(categoryId, page, pageSize);
        var pagedRes = new PagedResponse<Post> {
            Successful = true,
            Message = "Get posts list",
            Data = pagedList.ToList(),
            Pagination = pagedList.ToPaginationMetadata()
        };
        return Ok(pagedRes);
    }

    [HttpGet("{id}")]
    public ActionResult<Post> Get(string id) {
        var post = _postRepo.Where(a => a.Id == id).First();
        if (post == null) return NotFound();
        return post;
    }
}