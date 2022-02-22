using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Dtos;
using StarBlog.Web.ViewModels;
using StarBlog.Web.ViewModels.Response;

namespace StarBlog.Web.Apis;

/// <summary>
/// 置顶文章
/// </summary>
[ApiController]
[Route("Api/[controller]")]
public class TopPostController : ControllerBase {
    private readonly IBaseRepository<TopPost> _topPostRepo;
    private readonly IBaseRepository<Post> _postRepo;

    public TopPostController(IBaseRepository<TopPost> topPostRepo, IBaseRepository<Post> postRepo) {
        _topPostRepo = topPostRepo;
        _postRepo = postRepo;
    }

    [HttpGet]
    public ApiResponse<PostListDto> Get() {
        var postId = _topPostRepo.Select.Include(a => a.Post).First(a => a.Post.Id);
        var dto = _postRepo.Where(a => a.Id == postId).First<PostListDto>();
        return new ApiResponse<PostListDto> {Data = dto};
    }

    [HttpPut("[action]")]
    public ApiResponse Set([FromQuery] string postId) {
        var post = _postRepo.Where(a => a.Id == postId).First();
        if (post == null) return ApiResponse.NotFound(Response);

        var rows = _topPostRepo.Select.ToDelete().ExecuteAffrows();
        _topPostRepo.Insert(new TopPost {PostId = post.Id});
        return new ApiResponse {Successful = true, Message = $"ok. deleted {rows} old topPosts."};
    }

    [HttpDelete]
    public ApiResponse Clear() {
        var rows = _topPostRepo.Select.ToDelete().ExecuteAffrows();
        return new ApiResponse {Successful = true, Message = $"deleted {rows} topPosts."};
    }
}