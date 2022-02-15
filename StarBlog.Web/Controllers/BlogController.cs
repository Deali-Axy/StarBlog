using Microsoft.AspNetCore.Mvc;

namespace StarBlog.Web.Controllers; 

public class BlogController : Controller {
    // GET
    public IActionResult List() {
        return View();
    }
}