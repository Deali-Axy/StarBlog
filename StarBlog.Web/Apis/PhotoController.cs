using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;

namespace StarBlog.Web.Apis;

/// <summary>
/// 摄影
/// </summary>
[ApiController]
[Route("Api/[controller]")]
public class PhotoController : ControllerBase {
    private readonly IBaseRepository<Photo> _photoRepo;

    public PhotoController(IBaseRepository<Photo> photoRepo) {
        _photoRepo = photoRepo;
    }

    [HttpGet]
    public ActionResult<List<Photo>> GetAll() {
        return _photoRepo.Select.ToList();
    }

    [HttpGet("{id}")]
    public ActionResult<Photo> Get(string id) {
        var photo = _photoRepo.Where(a => a.Id == id).First();
        if (photo == null) return NotFound();
        return photo;
    }
}