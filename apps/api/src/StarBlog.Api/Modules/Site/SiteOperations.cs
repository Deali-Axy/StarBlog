using System.Text.RegularExpressions;
using StarBlog.Api.Infrastructure.Storage;
using StarBlog.Api.Modules.Configuration;
using StarBlog.Api.Modules.Content;
using StarBlog.Api.Modules.Links;
using StarBlog.Api.Modules.Media;
using StarBlog.Api.Shared;

namespace StarBlog.Api.Modules.Site;

/// <summary>
/// 首页聚合、搜索和主题列表。Site 只读取其他模块的公开契约。
/// </summary>
public sealed class SiteOperations {
    private readonly IPublishedContentQueries _content;
    private readonly IPhotoCatalog _photos;
    private readonly IPublicLinkCatalog _links;
    private readonly ISiteConfiguration _configuration;
    private readonly IAppPathProvider _paths;

    public SiteOperations(
        IPublishedContentQueries content,
        IPhotoCatalog photos,
        IPublicLinkCatalog links,
        ISiteConfiguration configuration,
        IAppPathProvider paths) {
        _content = content;
        _photos = photos;
        _links = links;
        _configuration = configuration;
        _paths = paths;
    }

    public async Task<HomeResponse> GetHomeAsync(CancellationToken cancellationToken) {
        var chart = await _configuration.GetAsync("home_chart_visible", cancellationToken);
        var randomPhoto = await _configuration.GetAsync("home_random_photo_visible", cancellationToken);
        return new HomeResponse {
            ChartVisible = string.Equals(chart, "true", StringComparison.OrdinalIgnoreCase),
            RandomPhotoVisible = string.Equals(randomPhoto, "true", StringComparison.OrdinalIgnoreCase),
            RandomPhoto = await _photos.GetRandomAsync(cancellationToken),
            TopPost = await _content.GetTopAsync(cancellationToken),
            FeaturedPosts = await _content.GetFeaturedPostsAsync(cancellationToken),
            FeaturedPhotos = await _photos.GetFeaturedAsync(cancellationToken),
            FeaturedCategories = await _content.GetFeaturedCategoriesAsync(cancellationToken),
            Links = await _links.GetVisibleAsync(cancellationToken)
        };
    }

    public async Task<IReadOnlyList<SearchResultResponse>> SearchAsync(string keyword, int page, int pageSize, CancellationToken cancellationToken) {
        page = Paging.NormalizePage(page);
        pageSize = Paging.NormalizePageSize(pageSize);
        var normalized = keyword.Trim();
        var pattern = new Regex(Regex.Escape(normalized), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        var posts = await _content.GetPublishedAsync(cancellationToken);
        return posts
            .Select(post => new {
                Post = post,
                Score = pattern.Matches(post.Title).Count * 3 + pattern.Matches(post.Content ?? string.Empty).Count
            })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Post.LastUpdateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new SearchResultResponse {
                Id = item.Post.Id,
                Title = item.Post.Title,
                Slug = item.Post.Slug,
                LastUpdateTime = item.Post.LastUpdateTime,
                HighlightedTitle = Highlight(pattern, item.Post.Title),
                HighlightedSnippet = BuildSnippet(pattern, item.Post.Content ?? string.Empty)
            })
            .ToList();
    }

    /// <summary>组合 Content 与 Media 的数量快照，Site 不直接读取其他模块实体。</summary>
    public async Task<SiteOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken) {
        var content = await _content.GetInventoryAsync(cancellationToken);
        return new SiteOverviewResponse {
            PostsCount = content.PostsCount,
            CategoriesCount = content.CategoriesCount,
            FeaturedPostsCount = content.FeaturedPostsCount,
            FeaturedCategoriesCount = content.FeaturedCategoriesCount,
            PhotosCount = await _photos.CountAsync(cancellationToken),
            FeaturedPhotosCount = await _photos.CountFeaturedAsync(cancellationToken)
        };
    }

    public IReadOnlyList<ThemeResponse> GetThemes() {
        var themes = new List<ThemeResponse> {
            new() { Name = "Bootstrap", Path = string.Empty, CssUrl = string.Empty }
        };
        var themePath = Path.Combine(_paths.WebRootPath, "lib", "bootswatch", "dist");
        if (!Directory.Exists(themePath)) return themes;
        foreach (var directory in Directory.GetDirectories(themePath)) {
            var name = Path.GetFileName(directory);
            themes.Add(new ThemeResponse {
                Name = name,
                Path = directory,
                CssUrl = $"/lib/bootswatch/dist/{name}/bootstrap.min.css"
            });
        }

        return themes;
    }

    private static string Highlight(Regex pattern, string value) =>
        pattern.Replace(value, match => $"<mark>{match.Value}</mark>");

    private static string BuildSnippet(Regex pattern, string content) {
        var match = pattern.Match(content);
        if (!match.Success) return content.Length <= 200 ? content : $"{content[..200]}...";
        var start = Math.Max(0, match.Index - 100);
        var length = Math.Min(content.Length - start, match.Length + 200);
        var snippet = Highlight(pattern, content.Substring(start, length));
        return $"{(start > 0 ? "..." : string.Empty)}{snippet}{(start + length < content.Length ? "..." : string.Empty)}";
    }
}
