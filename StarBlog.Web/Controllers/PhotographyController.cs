using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels;

namespace StarBlog.Web.Controllers; 

public class PhotographyController : Controller {
    private readonly IBaseRepository<Photo> _photoRepo;

    public PhotographyController(IBaseRepository<Photo> photoRepo) {
        _photoRepo = photoRepo;
    }

    public IActionResult Index() {
        return View(new PhotographyViewModel {
            Photos = _photoRepo.Select.ToList()
        });
    }
}