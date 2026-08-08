using AutoMapper;
using CodeLab.Share.Extensions;
using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Content.Utils;
using StarBlog.Data.Models;
using StarBlog.Api.Extensions;
using StarBlog.Application.Services;
using StarBlog.Application.ViewModels;
using StarBlog.Application.ViewModels.Blog;
using StarBlog.Application.Criteria;
using X.PagedList;

namespace StarBlog.Api.Apis.Blog;

/// <summary>
/// 文章
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[Route("Api/Admin/Posts")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
public class BlogPostController : ControllerBase {
    private readonly IMapper _mapper;
    private readonly PostService _postService;
    private readonly BlogService _blogService;

    public BlogPostController(PostService postService, BlogService blogService, IMapper mapper) {
        _postService = postService;
        _blogService = blogService;
        _mapper = mapper;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ApiResponsePaged<PostDto>> GetList([FromQuery] PostQueryParameters param) {
        // 已登录则设置为管理员模式
        // todo 后续改成根据角色确定管理员
        var adminMode = User.Identity?.IsAuthenticated ?? false;
        var pagedList = await _postService.GetPagedList(param, adminMode);
        return new ApiResponsePaged<PostDto> {
            Message = "Get posts list",
            Data = pagedList.Select(PostDto.From).ToList(),
            Pagination = pagedList.ToPaginationMetadata()
        };
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ApiResponse<PostDto>> Get(string id) {
        var post = await _postService.GetById(id);
        return post == null ? ApiResponse.NotFound() : new ApiResponse<PostDto>(PostDto.From(post));
    }

    [AllowAnonymous]
    [HttpGet("slug/{slug}")]
    public async Task<ApiResponse<PostDto>> GetBySlug(string slug) {
        var post = await _postService.GetBySlug(slug);
        return post == null ? ApiResponse.NotFound() : new ApiResponse<PostDto>(PostDto.From(post));
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse> Delete(string id) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");
        var rows = await _postService.Delete(id);
        return ApiResponse.Ok($"删除了 {rows} 篇博客");
    }

    // todo 发表文章需要同时设置已发布状态
    [HttpPost]
    public async Task<ApiResponse<PostDto>> Add(PostCreationDto dto,
        [FromServices] CategoryService categoryService) {
        var post = _mapper.Map<Post>(dto);
        var category = await categoryService.GetById(dto.CategoryId);
        if (category == null) return ApiResponse.BadRequest($"分类 {dto.CategoryId} 不存在！");

        if (!string.IsNullOrWhiteSpace(dto.Slug) && !await _postService.CheckSlugAvailable(dto.Slug)) {
            return ApiResponse.BadRequest("指定的 slug 已经被其他文章使用！");
        }

        post.Id = GuidUtils.GuidTo16String();
        post.CreationTime = DateTime.Now;
        post.LastUpdateTime = DateTime.Now;
        post.IsPublish = true;

        // 获取分类的层级结构
        post.Categories = categoryService.GetCategoryBreadcrumb(category);

        var saved = await _postService.InsertOrUpdateAsync(post);
        return new ApiResponse<PostDto>(PostDto.From(saved));
    }

    [HttpPut("{id}")]
    public async Task<ApiResponse<PostDto>> Update(string id, PostUpdateDto dto) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");

        if (!string.IsNullOrWhiteSpace(dto.Slug)) {
            if (dto.Slug!= post.Slug && !await _postService.CheckSlugAvailable(dto.Slug)) {
                return ApiResponse.BadRequest("指定的 slug 已经被其他文章使用！");
            }
        }

        // mapper.Map(source) 得到一个全新的对象
        // mapper.Map(source, dest) 在 dest 对象的基础上修改
        post = _mapper.Map(dto, post);
        post.LastUpdateTime = DateTime.Now;
        var saved = await _postService.InsertOrUpdateAsync(post);
        return new ApiResponse<PostDto>(PostDto.From(saved));
    }

    /// <summary>
    /// 上传图片
    /// </summary>
    /// <param name="id"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse> UploadImage(string id, IFormFile file) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");
        await using var stream = file.OpenReadStream();
        var imgUrl = await _postService.UploadImage(post, stream, file.FileName);
        return ApiResponse.Ok(new {
            imgUrl,
            imgName = Path.GetFileNameWithoutExtension(imgUrl)
        });
    }

    /// <summary>
    /// 获取文章里的图片
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/[action]")]
    public async Task<ApiResponse<List<string>>> Images(string id) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");
        return new ApiResponse<List<string>>(_postService.GetImages(post));
    }

    /// <summary>
    /// 设置为推荐博客
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse<FeaturedPostDto>> SetFeatured(string id) {
        var post = await _postService.GetById(id);
        return post == null
            ? ApiResponse.NotFound()
            : new ApiResponse<FeaturedPostDto>(FeaturedPostDto.From(await _blogService.AddFeaturedPost(post)));
    }

    /// <summary>
    /// 取消推荐博客
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse> CancelFeatured(string id) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");
        var rows = await _blogService.DeleteFeaturedPost(post);
        return ApiResponse.Ok($"delete {rows} rows.");
    }

    /// <summary>
    /// 设置置顶（只能有一篇置顶）
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse<TopPostDto>> SetTop(string id) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"博客 {id} 不存在");
        var (data, rows) = await _blogService.SetTopPost(post);
        return new ApiResponse<TopPostDto> { Data = TopPostDto.From(data), Message = $"ok. deleted {rows} old topPosts." };
    }

    /// <summary>翻译文章并保存指定语言版本。</summary>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse> Translate(string id, [FromServices] TranslationService translationService, [FromQuery] string language = "en") {
        if (await _postService.GetById(id) == null) return ApiResponse.NotFound($"博客 {id} 不存在");
        try {
            var translation = await translationService.TranslatePostAsync(id, language);
            return ApiResponse.Ok(new { translation.Id, translation.Language, translation.Title }, "文章翻译完成");
        }
        catch (Exception exception) {
            return ApiResponse.BadRequest($"翻译失败: {exception.Message}");
        }
    }

    /// <summary>获取指定语言的文章翻译。</summary>
    [AllowAnonymous]
    [HttpGet("{id}/[action]")]
    public async Task<ApiResponse<PostTranslation>> GetTranslation(string id, [FromServices] TranslationService translationService, [FromQuery] string language = "en") {
        var translation = await translationService.GetTranslation(id, language);
        return translation == null ? ApiResponse.NotFound($"未找到 {language} 翻译") : new ApiResponse<PostTranslation>(translation);
    }

    /// <summary>返回文章已生成的翻译语言代码。</summary>
    [AllowAnonymous]
    [HttpGet("{id}/[action]")]
    public async Task<ApiResponse<List<string>>> AvailableTranslations(string id, [FromServices] TranslationService translationService) =>
        new(await translationService.GetAvailableLanguages(id));

    /// <summary>删除文章的一个翻译版本。</summary>
    [HttpDelete("{id}/[action]/{translationId}")]
    public async Task<ApiResponse> DeleteTranslation(string id, string translationId, [FromServices] TranslationService translationService) {
        var rows = await translationService.DeleteTranslation(translationId);
        return rows > 0 ? ApiResponse.Ok("翻译已删除") : ApiResponse.NotFound("翻译不存在");
    }
}
