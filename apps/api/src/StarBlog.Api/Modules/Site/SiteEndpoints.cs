using Microsoft.AspNetCore.Mvc;
using StarBlog.Api.Modules.Media;
using StarBlog.Api.Shared;

namespace StarBlog.Api.Modules.Site;

/// <summary>
/// 首页、搜索、主题、相邻图片与站点资源。
/// </summary>
public static class SiteEndpoints {
    public static IEndpointRouteBuilder MapSiteEndpoints(this IEndpointRouteBuilder endpoints) {
        var site = endpoints.MapGroup("/api/v1/site").WithTags("common").WithGroupName("common");
        site.MapGet("/home", Home).AllowAnonymous();
        site.MapGet("/search", Search).AllowAnonymous();
        site.MapGet("/photos/random", RandomPhoto).AllowAnonymous();
        site.MapGet("/photos/{id}/adjacent/next", NextPhoto).AllowAnonymous();
        site.MapGet("/photos/{id}/adjacent/previous", PreviousPhoto).AllowAnonymous();

        endpoints.MapGet("/api/v1/theme", Themes).AllowAnonymous().WithTags("common").WithGroupName("common");
        endpoints.MapGet("/feed", Feed).AllowAnonymous().WithTags("common").WithGroupName("common");
        endpoints.MapGet("/robots.txt", Robots).AllowAnonymous().WithTags("common").WithGroupName("common");
        endpoints.MapGet("/sitemap-index.xml", SitemapIndex).AllowAnonymous().WithTags("common").WithGroupName("common");
        endpoints.MapGet("/sitemap.xml", Sitemap).AllowAnonymous().WithTags("common").WithGroupName("common");
        endpoints.MapGet("/sitemap-images.xml", ImageSitemap).AllowAnonymous().WithTags("common").WithGroupName("common");
        return endpoints;
    }

    private static Task<HomeResponse> Home(SiteOperations operations, CancellationToken cancellationToken) =>
        operations.GetHomeAsync(cancellationToken);

    private static async Task<IResult> Search(
        SiteOperations operations,
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) {
        if (string.IsNullOrWhiteSpace(keyword)) return HttpErrors.BadRequest("keyword 不能为空");
        return Results.Ok(await operations.SearchAsync(keyword, page, pageSize, cancellationToken));
    }

    private static async Task<IResult> RandomPhoto(IPhotoCatalog photos, CancellationToken cancellationToken) {
        var photo = await photos.GetRandomAsync(cancellationToken);
        return photo == null ? HttpErrors.NotFound("当前没有图片") : Results.Ok(photo);
    }

    private static async Task<IResult> NextPhoto(string id, IPhotoCatalog photos, CancellationToken cancellationToken) {
        var photo = await photos.GetNextAsync(id, cancellationToken);
        return photo == null ? HttpErrors.NotFound("没有下一张图片") : Results.Ok(photo);
    }

    private static async Task<IResult> PreviousPhoto(string id, IPhotoCatalog photos, CancellationToken cancellationToken) {
        var photo = await photos.GetPreviousAsync(id, cancellationToken);
        return photo == null ? HttpErrors.NotFound("没有上一张图片") : Results.Ok(photo);
    }

    private static IReadOnlyList<ThemeResponse> Themes(SiteOperations operations) => operations.GetThemes();

    private static async Task<IResult> Feed(SiteResourceOperations operations, CancellationToken cancellationToken) =>
        Results.File(await operations.BuildFeedAsync(cancellationToken), "application/atom+xml; charset=utf-8");

    private static async Task<IResult> Robots(SiteResourceOperations operations, CancellationToken cancellationToken) =>
        Results.Text(await operations.BuildRobotsAsync(cancellationToken), "text/plain; charset=utf-8");

    private static async Task<IResult> SitemapIndex(SiteResourceOperations operations, CancellationToken cancellationToken) =>
        Results.File(await operations.BuildSitemapIndexAsync(cancellationToken), "application/xml; charset=utf-8");

    private static async Task<IResult> Sitemap(SiteResourceOperations operations, CancellationToken cancellationToken) =>
        Results.File(await operations.BuildSitemapAsync(cancellationToken), "application/xml; charset=utf-8");

    private static async Task<IResult> ImageSitemap(SiteResourceOperations operations, CancellationToken cancellationToken) =>
        Results.File(await operations.BuildImageSitemapAsync(cancellationToken), "application/xml; charset=utf-8");
}
