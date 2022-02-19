using Microsoft.AspNetCore.Mvc;

namespace StarBlog.Web.Controllers; 

public class ProductController : Controller {
    public IActionResult Index() {
        return View();
    }
}