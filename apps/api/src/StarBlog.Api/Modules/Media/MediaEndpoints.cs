using Microsoft.AspNetCore.Mvc;
using StarBlog.Api.Shared;

namespace StarBlog.Api.Modules.Media;

/// <summary>
/// 摄影作品与精选图片 HTTP 边界。
/// </summary>
public static class MediaEndpoints {
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder endpoints) {
        var photos = endpoints.MapGroup("/api/v1/photos").WithTags("photo").WithGroupName("photo");
        photos.MapGet("/", ListAsync).AllowAnonymous();
        photos.MapGet("/{id}", GetAsync).AllowAnonymous();
        photos.MapGet("/{id}/thumbnail", GetThumbAsync).AllowAnonymous();
        photos.MapPost("/", CreateAsync).RequireAuthorization().DisableAntiforgery();
        photos.MapPut("/{id}", UpdateAsync).RequireAuthorization();
        photos.MapDelete("/{id}", DeleteAsync).RequireAuthorization();
        photos.MapPost("/{id}/featured-photo", SetFeaturedAsync).RequireAuthorization();
        photos.MapDelete("/{id}/featured-photo", CancelFeaturedAsync).RequireAuthorization();

        endpoints.MapGet("/api/v1/featured-photos", GetFeaturedAsync)
            .AllowAnonymous()
            .WithTags("photo")
            .WithGroupName("photo");
        return endpoints;
    }

    private static Task<PageResult<PhotoResponse>> ListAsync(
        MediaOperations operations,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        operations.GetPagedAsync(page, pageSize, cancellationToken);

    private static async Task<IResult> GetAsync(string id, MediaOperations operations, CancellationToken cancellationToken) {
        var photo = await operations.GetByIdAsync(id, cancellationToken);
        return photo == null ? HttpErrors.NotFound($"图片 {id} 不存在") : Results.Ok(photo);
    }

    private static async Task<IResult> GetThumbAsync(
        string id,
        MediaOperations operations,
        [FromQuery] int width = 300,
        [FromQuery] int quality = 85,
        CancellationToken cancellationToken = default) {
        if (width is < 1 or > 2000) return HttpErrors.BadRequest("width 参数范围为 1-2000");
        if (quality is < 1 or > 100) return HttpErrors.BadRequest("quality 参数范围为 1-100");
        var data = await operations.GetThumbAsync(id, width, quality, cancellationToken);
        return data == null ? HttpErrors.NotFound($"图片 {id} 不存在") : Results.File(data, "image/jpeg");
    }

    private static async Task<IResult> CreateAsync(
        HttpRequest request,
        MediaOperations operations,
        CancellationToken cancellationToken) {
        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.GetFile("file");
        if (file == null) return HttpErrors.BadRequest("请上传图片文件");
        var title = form["title"].ToString();
        var location = form["location"].ToString();
        await using var stream = file.OpenReadStream();
        var photo = await operations.AddAsync(title, location, stream, cancellationToken);
        return Results.Ok(photo);
    }

    private static async Task<IResult> UpdateAsync(
        string id,
        [FromBody] PhotoUpdateRequest request,
        MediaOperations operations,
        CancellationToken cancellationToken) {
        var photo = await operations.UpdateAsync(id, request, cancellationToken);
        return photo == null ? HttpErrors.NotFound($"图片 {id} 不存在") : Results.Ok(photo);
    }

    private static async Task<IResult> DeleteAsync(string id, MediaOperations operations, CancellationToken cancellationToken) {
        var deleted = await operations.DeleteAsync(id, cancellationToken);
        return deleted ? Results.NoContent() : HttpErrors.NotFound($"图片 {id} 不存在");
    }

    private static async Task<IResult> SetFeaturedAsync(string id, MediaOperations operations, CancellationToken cancellationToken) {
        var featured = await operations.SetFeaturedAsync(id, cancellationToken);
        return featured == null ? HttpErrors.NotFound($"图片 {id} 不存在") : Results.Ok(featured);
    }

    private static async Task<IResult> CancelFeaturedAsync(string id, MediaOperations operations, CancellationToken cancellationToken) {
        var cancelled = await operations.CancelFeaturedAsync(id, cancellationToken);
        return cancelled ? Results.NoContent() : HttpErrors.NotFound($"图片 {id} 不存在或未推荐");
    }

    private static Task<IReadOnlyList<PhotoResponse>> GetFeaturedAsync(MediaOperations operations, CancellationToken cancellationToken) =>
        operations.GetFeaturedAsync(cancellationToken);
}
