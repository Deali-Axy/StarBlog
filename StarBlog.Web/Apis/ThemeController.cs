using Microsoft.AspNetCore.Mvc;
using StarBlog.Web.Services;

namespace StarBlog.Web.Apis; 

[ApiController]
[Route("Api/[controller]")]
public class ThemeController : ControllerBase {
    private readonly ThemeService _themeService;

    public ThemeController(ThemeService themeService) {
        _themeService = themeService;
    }

    [HttpGet]
    public ActionResult<List<Theme>> GetAll() {
        return _themeService.Themes;
    }
}