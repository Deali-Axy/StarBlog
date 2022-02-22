using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels;
using StarBlog.Web.ViewModels.Response;

namespace StarBlog.Web.Apis;

/// <summary>
/// 推荐博客
/// </summary>
[ApiController]
[Route("Api/[controller]")]
public class FeaturedPostController : ControllerBase {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<FeaturedPost> _featuredPostRepo;

    public FeaturedPostController(IBaseRepository<FeaturedPost> featuredPostRepo, IBaseRepository<Post> postRepo) {
        _featuredPostRepo = featuredPostRepo;
        _postRepo = postRepo;
    }

    [HttpGet]
    public ApiResponse<List<Post>> GetList() {
        return new ApiResponse<List<Post>> {
            Data = _featuredPostRepo.Select.Include(a => a.Post.Category)
                .ToList(a => a.Post)
        };
    }

    [HttpGet("{id:int}")]
    public ApiResponse<Post> Get(int id) {
        var item = _featuredPostRepo.Where(a => a.Id == id)
            .Include(a => a.Post).First();
        return item == null ? ApiResponse<Post>.NotFound(Response) : new ApiResponse<Post> {Data = item.Post};
    }

    [HttpPost]
    public ApiResponse Add([FromQuery] string postId) {
        var post = _postRepo.Where(a => a.Id == postId).First();
        if (post == null) return ApiResponse.NotFound(Response);
        _featuredPostRepo.Insert(new FeaturedPost {PostId = postId});
        return ApiResponse.Ok(Response);
    }

    [HttpDelete("{id:int}")]
    public ApiResponse Delete(int id) {
        var item = _featuredPostRepo.Where(a => a.Id == id).First();
        if (item == null) return ApiResponse.NotFound(Response);
        var rows = _featuredPostRepo.Delete(item);
        return ApiResponse.Ok(Response, $"deleted {rows} rows.");
    }
}