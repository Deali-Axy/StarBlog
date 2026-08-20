using CodeLab.Share.ViewModels.Response;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Api.Extensions;
using StarBlog.Application.ViewModels.Blog;

namespace StarBlog.Api.Apis.Blog;

/// <summary>
/// 推荐博客
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/featured-posts")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
public class FeaturedPostController : ControllerBase {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<FeaturedPost> _featuredPostRepo;

    public FeaturedPostController(IBaseRepository<FeaturedPost> featuredPostRepo, IBaseRepository<Post> postRepo) {
        _featuredPostRepo = featuredPostRepo;
        _postRepo = postRepo;
    }

    [AllowAnonymous]
    [HttpGet]
    public ApiResponse<List<FeaturedPostDto>> GetList() {
        var data = _featuredPostRepo.Select
            .Include(a => a.Post.Category)
            .ToList()
            .Select(FeaturedPostDto.From)
            .ToList();
        return new ApiResponse<List<FeaturedPostDto>>(data);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public ApiResponse<FeaturedPostDto> Get(int id) {
        var item = _featuredPostRepo.Where(a => a.Id == id)
            .Include(a => a.Post.Category).First();
        return item == null ? ApiResponse.NotFound() : new ApiResponse<FeaturedPostDto>(FeaturedPostDto.From(item));
    }

    [HttpPost]
    public ApiResponse<FeaturedPostDto> Add([FromQuery] string postId) {
        var post = _postRepo.Where(a => a.Id == postId).First();
        if (post == null) return ApiResponse.NotFound($"博客 {postId} 不存在");
        var item = _featuredPostRepo.Insert(new FeaturedPost { PostId = postId });
        item.Post = post;
        return new ApiResponse<FeaturedPostDto>(FeaturedPostDto.From(item));
    }

    [HttpDelete("{id:int}")]
    public ApiResponse Delete(int id) {
        var item = _featuredPostRepo.Where(a => a.Id == id).First();
        if (item == null) return ApiResponse.NotFound($"推荐博客记录 {id} 不存在");
        var rows = _featuredPostRepo.Delete(item);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }
}
