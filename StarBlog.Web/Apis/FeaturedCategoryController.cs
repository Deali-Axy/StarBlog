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
public class FeaturedCategoryController : ControllerBase {
    private readonly CategoryService _categoryService;

    public FeaturedCategoryController(CategoryService categoryService) {
        _categoryService = categoryService;
    }

    [HttpPost]
    public ApiResponse<List<FeaturedCategory>> GetList() {
        return new ApiResponse<List<FeaturedCategory>>(_categoryService.GetFeaturedCategories());
    }
}