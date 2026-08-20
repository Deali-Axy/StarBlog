using System.Text.RegularExpressions;
using CodeLab.Share.ViewModels.Response;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Api.Extensions;
using StarBlog.Application.Services;
using StarBlog.Application.ViewModels.Blog;
using StarBlog.Application.ViewModels.Site;
using StarBlog.Content.Extensions;
using StarBlog.Data.Models;

namespace StarBlog.Api.Apis.Common;

/// <summary>
/// 替代旧 Razor 页面控制器的前台站点 API。
/// 所有页面组装逻辑均以 JSON 返回，渲染和跳转交由新的前端负责。
/// </summary>
[ApiController]
[Route("api/v1/site")]
[ApiExplorerSettings(GroupName = ApiGroups.Common)]
public sealed class SiteController : ControllerBase {
    private readonly BlogService _blogService;
    private readonly CategoryService _categoryService;
    private readonly PhotoService _photoService;
    private readonly LinkService _linkService;
    private readonly LinkExchangeService _linkExchangeService;
    private readonly ConfigService _configService;
    private readonly IBaseRepository<Post> _postRepository;
    private readonly IBaseRepository<User> _userRepository;

    public SiteController(
        BlogService blogService,
        CategoryService categoryService,
        PhotoService photoService,
        LinkService linkService,
        LinkExchangeService linkExchangeService,
        ConfigService configService,
        IBaseRepository<Post> postRepository,
        IBaseRepository<User> userRepository) {
        _blogService = blogService;
        _categoryService = categoryService;
        _photoService = photoService;
        _linkService = linkService;
        _linkExchangeService = linkExchangeService;
        _configService = configService;
        _postRepository = postRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// 获取原首页所需的聚合数据，避免新前端为一个页面发起多次串行请求。
    /// </summary>
    [AllowAnonymous]
    [HttpGet("home")]
    public async Task<ApiResponse<object>> Home() {
        var topPost = await _blogService.GetTopOnePost();
        return ApiResponse.Ok(new {
            ChartVisible = string.Equals(_configService["home_chart_visible"], "true", StringComparison.OrdinalIgnoreCase),
            RandomPhotoVisible = string.Equals(_configService["home_random_photo_visible"], "true", StringComparison.OrdinalIgnoreCase),
            RandomPhoto = await _photoService.GetRandomPhoto(),
            TopPost = topPost == null ? null : PostDto.From(topPost),
            FeaturedPosts = (await _blogService.GetFeaturedPosts()).Select(PostDto.From).ToList(),
            FeaturedPhotos = await _photoService.GetFeaturedPhotos(),
            FeaturedCategories = await _categoryService.GetFeaturedCategories(),
            Links = await _linkService.GetAll(true)
        });
    }

    /// <summary>
    /// 搜索已发布文章，并在标题和摘要片段中标记命中的关键词。
    /// </summary>
    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<ApiResponse<List<SearchResultItemDto>>> Search([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) {
        if (string.IsNullOrWhiteSpace(keyword)) return ApiResponse.BadRequest("keyword 不能为空");

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var normalizedKeyword = keyword.Trim();
        var posts = await _postRepository
            .Where(post => post.IsPublish && ((post.Title ?? string.Empty).Contains(normalizedKeyword) || (post.Content ?? string.Empty).Contains(normalizedKeyword)))
            .ToListAsync();
        var pattern = new Regex(Regex.Escape(normalizedKeyword), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        var result = posts
            .Select(post => new {
                Post = post,
                Score = pattern.Matches(post.Title ?? string.Empty).Count * 3 + pattern.Matches(post.Content ?? string.Empty).Count
            })
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Post.LastUpdateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new SearchResultItemDto {
                Id = item.Post.Id,
                Title = item.Post.Title ?? string.Empty,
                Slug = item.Post.Slug,
                LastUpdateTime = item.Post.LastUpdateTime,
                HighlightedTitle = Highlight(pattern, item.Post.Title ?? string.Empty),
                HighlightedSnippet = BuildSnippet(pattern, item.Post.Content ?? string.Empty)
            })
            .ToList();
        return new ApiResponse<List<SearchResultItemDto>>(result);
    }

    /// <summary>获取相邻照片，供前端实现上一张/下一张导航。</summary>
    [AllowAnonymous]
    [HttpGet("photos/{id}/adjacent/next")]
    public async Task<ApiResponse<Photo>> NextPhoto(string id) {
        var photo = await _photoService.GetNext(id);
        return photo == null ? ApiResponse.NotFound("没有下一张图片") : new ApiResponse<Photo>(photo);
    }

    /// <summary>获取相邻照片，供前端实现上一张/下一张导航。</summary>
    [AllowAnonymous]
    [HttpGet("photos/{id}/adjacent/previous")]
    public async Task<ApiResponse<Photo>> PreviousPhoto(string id) {
        var photo = await _photoService.GetPrevious(id);
        return photo == null ? ApiResponse.NotFound("没有上一张图片") : new ApiResponse<Photo>(photo);
    }

    /// <summary>随机返回一张照片。</summary>
    [AllowAnonymous]
    [HttpGet("photos/random")]
    public async Task<ApiResponse<Photo>> RandomPhoto() {
        var photo = await _photoService.GetRandomPhoto();
        return photo == null ? ApiResponse.NotFound("当前没有图片") : new ApiResponse<Photo>(photo);
    }

    /// <summary>提交友情链接申请；审核由管理端的链接交换申请资源完成。</summary>
    [AllowAnonymous]
    [HttpPost("link-exchanges")]
    public async Task<ApiResponse<LinkExchange>> ApplyForLinkExchange([FromBody] LinkExchangeApplicationDto dto) {
        if (!ModelState.IsValid) return ApiResponse.BadRequest(ModelState);
        if (await _linkExchangeService.HasUrl(dto.Url)) return ApiResponse.BadRequest("相同网址的友链申请已提交");

        var application = await _linkExchangeService.AddOrUpdate(new LinkExchange {
            Name = dto.Name,
            Description = dto.Description,
            Url = dto.Url,
            WebMaster = dto.WebMaster,
            Email = dto.Email,
            Verified = false
        });
        await _linkExchangeService.SendEmailOnAdd(application);
        return new ApiResponse<LinkExchange>(application) { Message = "友链申请已提交，正在处理中" };
    }

    /// <summary>
    /// 查询是否已经完成首次初始化；以 SQLite 用户表中的管理员记录为准。
    /// 这样即使管理员由运维人员直接写入数据库，管理后台也能立即识别初始化状态。
    /// </summary>
    [AllowAnonymous]
    [HttpGet("initialization")]
    public ApiResponse<object> GetInitializationState() => new(new {
        IsInitialized = _userRepository.Select.Any(),
        Host = _configService["host"],
        DefaultRender = _configService["default_render"]
    });

    /// <summary>
    /// 仅允许在 SQLite 用户表为空时创建首个管理员。
    /// 密码仅保存 SHA-256 哈希以兼容现有登录逻辑，接口成功后不可再次调用。
    /// </summary>
    [AllowAnonymous]
    [HttpPost("initialization")]
    public ApiResponse Initialize([FromBody] InitializeSiteDto dto) {
        if (!ModelState.IsValid) return ApiResponse.BadRequest(ModelState);
        if (_userRepository.Select.Any()) {
            return ApiResponse.BadRequest("站点已经完成初始化");
        }

        // 先写管理员，再更新初始化标记；即使后续配置写入失败，也不会出现标记已完成但没有账号的锁死状态。
        _userRepository.Insert(new User {
            Id = Guid.NewGuid().ToString("N"),
            Name = dto.Username.Trim(),
            Password = dto.Password.ToSHA256()
        });
        _configService["host"] = dto.Host.TrimEnd('/');
        _configService["default_render"] = dto.DefaultRender;
        _configService["is_init"] = "true";
        return ApiResponse.Ok("初始化完成");
    }

    private static string Highlight(Regex pattern, string value) => pattern.Replace(value, match => $"<mark>{match.Value}</mark>");

    private static string BuildSnippet(Regex pattern, string content) {
        var match = pattern.Match(content);
        if (!match.Success) return content.Length <= 200 ? content : $"{content[..200]}...";

        var start = Math.Max(0, match.Index - 100);
        var length = Math.Min(content.Length - start, match.Length + 200);
        var snippet = Highlight(pattern, content.Substring(start, length));
        return $"{(start > 0 ? "..." : string.Empty)}{snippet}{(start + length < content.Length ? "..." : string.Empty)}";
    }
}
