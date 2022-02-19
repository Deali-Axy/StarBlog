using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;

namespace StarBlog.Web.Apis;

/// <summary>
/// 文章
/// </summary>
[ApiController]
[Route("Api/[controller]")]
public class BlogPostController : ControllerBase {
    private readonly IBaseRepository<Post> _postRepo;


    public BlogPostController(IBaseRepository<Post> postRepo, IBaseRepository<TopPost> topPostRepo,
        IBaseRepository<FeaturedPost> featuredPostRepo) {
        _postRepo = postRepo;
    }

    [HttpGet]
    public ActionResult<List<Post>> GetAll() {
        return _postRepo.Select.Include(a => a.Category).ToList();
    }

    [HttpGet("{id}")]
    public ActionResult<Post> Get(string id) {
        var post = _postRepo.Where(a => a.Id == id).First();
        if (post == null) return NotFound();
        return post;
    }
}