using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;

namespace StarBlog.Web.Apis;

[ApiController]
[Route("Api/[controller]")]
public class CategoryController : ControllerBase {
    private IBaseRepository<Category> _categoryRepo;

    public CategoryController(IBaseRepository<Category> categoryRepo) {
        _categoryRepo = categoryRepo;
    }

    [HttpGet]
    public ActionResult<List<Category>> GetAll() {
        return _categoryRepo.Select.ToList();
    }

    [HttpGet("{id}")]
    public ActionResult<Category> Get(int id) {
        var item = _categoryRepo.Where(a => a.Id == id).First();
        if (item == null) return NotFound();
        return item;
    }

    [HttpGet("[action]")]
    public ActionResult<List<object>> WordCloud() {
        var list = _categoryRepo.Select.IncludeMany(a => a.Posts).ToList();
        var data = new List<object>();
        foreach (var item in list) {
            data.Add(new {name=item.Name,value=item.Posts.Count});
        }

        return data;
    }
}