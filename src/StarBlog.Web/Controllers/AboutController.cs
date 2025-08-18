using Microsoft.AspNetCore.Mvc;

namespace StarBlog.Web.Controllers; 

public class AboutController : Controller {
    // GET
    public IActionResult Index() {
        return View();
    }
}