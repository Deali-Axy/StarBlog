using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Categories;
using StarBlog.Web.ViewModels.Response;

namespace StarBlog.Web.Apis;

/// <summary>
/// 文章分类
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = "blog")]
public class CategoryController : ControllerBase {
    private readonly CategoryService _cService;

    public CategoryController(CategoryService cService) {
        _cService = cService;
    }

    [AllowAnonymous]
    [HttpGet("All")]
    public ApiResponse<List<Category>> GetAll() {
        return new ApiResponse<List<Category>>(_cService.GetAll());
    }

    [AllowAnonymous]
    [HttpGet]
    public ApiResponsePaged<Category> GetList(int page = 1, int pageSize = 10) {
        var paged = _cService.GetPagedList(page, pageSize);
        return new ApiResponsePaged<Category> {
            Pagination = paged.ToPaginationMetadata(),
            Data = paged.ToList()
        };
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IApiResponse Get(int id) {
        var item = _cService.GetById(id);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<Category> {Data = item};
    }

    /// <summary>
    /// 分类词云
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("[action]")]
    public ApiResponse<List<object>> WordCloud() {
        return new ApiResponse<List<object>>(_cService.GetWordCloud());
    }

    /// <summary>
    /// 设置为推荐分类
    /// </summary>
    /// <seealso href="https://fontawesome.com/search?m=free">FontAwesome图标库</seealso>
    /// <param name="id"></param>
    /// <param name="dto">推荐信息 <see cref="FeaturedCategoryCreationDto"/></param>
    /// <returns></returns>
    [HttpPost("{id:int}/[action]")]
    public ApiResponse<FeaturedCategory> SetFeatured(int id, [FromBody] FeaturedCategoryCreationDto dto) {
        var item = _cService.GetById(id);
        return item == null
            ? ApiResponse.NotFound($"分类 {id} 不存在")
            : new ApiResponse<FeaturedCategory>(_cService.AddOrUpdateFeaturedCategory(item, dto));
    }

    /// <summary>
    /// 取消推荐分类
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id:int}/[action]")]
    public ApiResponse CancelFeatured(int id) {
        var item = _cService.GetById(id);
        if (item == null) return ApiResponse.NotFound($"分类 {id} 不存在");
        var rows = _cService.DeleteFeaturedCategory(item);
        return ApiResponse.Ok($"delete {rows} rows.");
    }

    /// <summary>
    /// 设置分类可见
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id:int}/[action]")]
    public ApiResponse SetVisible(int id) {
        var item = _cService.GetById(id);
        if (item == null) return ApiResponse.NotFound($"分类 {id} 不存在");
        var rows = _cService.SetVisibility(item, true);
        return ApiResponse.Ok($"affect {rows} rows.");
    }

    /// <summary>
    /// 设置分类不可见
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id:int}/[action]")]
    public ApiResponse SetInvisible(int id) {
        var item = _cService.GetById(id);
        if (item == null) return ApiResponse.NotFound($"分类 {id} 不存在");
        var rows = _cService.SetVisibility(item, false);
        return ApiResponse.Ok($"affect {rows} rows.");
    }
}