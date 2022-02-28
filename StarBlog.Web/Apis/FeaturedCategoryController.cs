using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Response;

namespace StarBlog.Web.Apis;

/// <summary>
/// 推荐分类
/// </summary>
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = "blog")]
public class FeaturedCategoryController : ControllerBase {
    private readonly CategoryService _categoryService;

    public FeaturedCategoryController(CategoryService categoryService) {
        _categoryService = categoryService;
    }

    [HttpGet]
    public ApiResponse<List<FeaturedCategory>> GetList() {
        return new ApiResponse<List<FeaturedCategory>>(_categoryService.GetFeaturedCategories());
    }
}