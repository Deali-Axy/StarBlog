using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels;

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
    public Response<Post> Get() {
        var item = _topPostRepo.Select.Include(a => a.Post).First(a => a.Post);
        return new Response<Post> {Data = item};
    }

    [HttpPut("[action]")]
    public Response Set([FromQuery] string postId) {
        var post = _postRepo.Where(a => a.Id == postId).First();
        if (post == null) {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return new Response {Successful = false, Message = "post not found"};
        }

        var rows = _topPostRepo.Select.ToDelete().ExecuteAffrows();
        _topPostRepo.Insert(new TopPost {PostId = post.Id});
        return new Response {Successful = true, Message = $"ok. deleted {rows} old topPosts."};
    }

    [HttpDelete]
    public Response Clear() {
        var rows = _topPostRepo.Select.ToDelete().ExecuteAffrows();
        return new Response {Successful = true, Message = $"deleted {rows} topPosts."};
    }
}