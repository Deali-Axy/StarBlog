using Microsoft.AspNetCore.Mvc;
using StarBlog.Api.Extensions;
using StarBlog.Application.Services;

namespace StarBlog.Api.Apis.Common; 

/// <summary>
/// 页面主题
/// </summary>
[ApiController]
[Route("api/v1/theme")]
[ApiExplorerSettings(GroupName = ApiGroups.Common)]
public class ThemeController : ControllerBase {
    private readonly ThemeService _themeService;

    public ThemeController(ThemeService themeService) {
        _themeService = themeService;
    }

    [HttpGet]
    public List<Theme> GetAll() {
        return _themeService.Themes;
    }
}
