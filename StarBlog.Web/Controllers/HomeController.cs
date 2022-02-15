using Microsoft.AspNetCore.Mvc;

namespace StarBlog.Web.Controllers; 

public class HomeController : Controller {
    // GET
    public IActionResult Index() {
        return View();
    }
}