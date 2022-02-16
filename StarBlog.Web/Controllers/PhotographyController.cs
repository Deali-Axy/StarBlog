using Microsoft.AspNetCore.Mvc;

namespace StarBlog.Web.Controllers; 

public class PhotographyController : Controller {
    // GET
    public IActionResult Index() {
        return View();
    }
}