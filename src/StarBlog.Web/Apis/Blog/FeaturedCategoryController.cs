using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Categories;

namespace StarBlog.Web.Apis.Blog;

/// <summary>
/// 推荐分类
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
public class FeaturedCategoryController : ControllerBase {
    private readonly CategoryService _categoryService;

    public FeaturedCategoryController(CategoryService categoryService) {
        _categoryService = categoryService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<List<FeaturedCategory>> GetAll() {
        return await _categoryService.GetFeaturedCategories();
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ApiResponse<FeaturedCategory>> Get(int id) {
        var item = await _categoryService.GetFeaturedCategoryById(id);
        return item == null
            ? ApiResponse.NotFound($"推荐分类记录 {id} 不存在")
            : new ApiResponse<FeaturedCategory>(item);
    }

    [HttpPost]
    public async Task<ApiResponse<FeaturedCategory>> Add(FeaturedCategoryCreationDto2 dto2) {
        var category = await _categoryService.GetById(dto2.CategoryId);
        if (category == null) return ApiResponse.NotFound($"分类 {dto2.CategoryId} 不存在");
        var item = await _categoryService.AddOrUpdateFeaturedCategory(category, new FeaturedCategoryCreationDto {
            Name = dto2.Name, Description = dto2.Description, IconCssClass = dto2.IconCssClass
        });
        return new ApiResponse<FeaturedCategory>(item);
    }

    [HttpDelete("{id:int}")]
    public ApiResponse Delete(int id) {
        var item = _categoryService.GetFeaturedCategoryById(id);
        if (item == null) return ApiResponse.NotFound($"推荐分类记录 {id} 不存在");
        var rows = _categoryService.DeleteFeaturedCategoryById(id);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }
}