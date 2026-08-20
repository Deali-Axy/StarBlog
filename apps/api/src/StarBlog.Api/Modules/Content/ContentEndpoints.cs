using Microsoft.AspNetCore.Mvc;
using StarBlog.Api.Shared;

namespace StarBlog.Api.Modules.Content;

/// <summary>
/// 文章、分类、精选、翻译与发布 HTTP 边界。
/// </summary>
public static class ContentEndpoints {
    public static IEndpointRouteBuilder MapContentEndpoints(this IEndpointRouteBuilder endpoints) {
        var posts = endpoints.MapGroup("/api/v1/posts").WithTags("blog").WithGroupName("blog");
        posts.MapGet("/", ListPosts).AllowAnonymous();
        posts.MapGet("/{id}", GetPost).AllowAnonymous();
        posts.MapPost("/", CreatePost).RequireAuthorization();
        posts.MapPut("/{id}", UpdatePost).RequireAuthorization();
        posts.MapDelete("/{id}", DeletePost).RequireAuthorization();
        posts.MapPost("/{id}/images", UploadImage).RequireAuthorization().DisableAntiforgery();
        posts.MapPost("/{id}/featured-post", FeaturePost).RequireAuthorization();
        posts.MapPut("/{id}/top-placement", TopPost).RequireAuthorization();
        posts.MapPost("/{id}/translations", Translate).RequireAuthorization();

        var categories = endpoints.MapGroup("/api/v1/categories").WithTags("blog").WithGroupName("blog");
        categories.MapGet("/", ListCategories).AllowAnonymous();
        categories.MapGet("/tree", CategoryTree).AllowAnonymous();
        categories.MapPost("/", CreateCategory).RequireAuthorization();
        categories.MapPut("/{id:int}", UpdateCategory).RequireAuthorization();
        categories.MapDelete("/{id:int}", DeleteCategory).RequireAuthorization();
        categories.MapPost("/{id:int}/featured-category", FeatureCategory).RequireAuthorization();

        endpoints.MapGet("/api/v1/site/overview", Overview)
            .AllowAnonymous()
            .WithTags("blog")
            .WithGroupName("blog");

        var channels = endpoints.MapGroup("/api/v1/admin/publication-channels")
            .RequireAuthorization()
            .WithTags("admin")
            .WithGroupName("admin");
        channels.MapGet("/", ListChannels);
        channels.MapPost("/", SaveChannel);

        endpoints.MapPost("/api/v1/admin/posts/{postId}/publications", PreparePublication)
            .RequireAuthorization()
            .WithTags("admin")
            .WithGroupName("admin");
        endpoints.MapPost("/api/v1/admin/publications/{id}/deliveries", DeliverPublication)
            .RequireAuthorization()
            .WithTags("admin")
            .WithGroupName("admin");
        return endpoints;
    }

    private static Task<PageResult<PostResponse>> ListPosts(
        ContentOperations operations,
        HttpContext context,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] bool? isPublish = null,
        CancellationToken cancellationToken = default) =>
        operations.GetPostsAsync(page, pageSize, context.User.Identity?.IsAuthenticated == true, search, categoryId, isPublish, cancellationToken);

    private static async Task<IResult> GetPost(string id, ContentOperations operations, CancellationToken cancellationToken) {
        var post = await operations.GetPostAsync(id, cancellationToken);
        return post == null ? HttpErrors.NotFound($"文章 {id} 不存在") : Results.Ok(post);
    }

    private static async Task<IResult> CreatePost([FromBody] PostUpsertRequest request, ContentOperations operations, CancellationToken cancellationToken) {
        var (post, error) = await operations.CreatePostAsync(request, cancellationToken);
        if (error != null) return HttpErrors.BadRequest(error);
        return Results.Ok(post);
    }

    private static async Task<IResult> UpdatePost(string id, [FromBody] PostUpsertRequest request, ContentOperations operations, CancellationToken cancellationToken) {
        var (post, error) = await operations.UpdatePostAsync(id, request, cancellationToken);
        if (error != null) return HttpErrors.BadRequest(error);
        return post == null ? HttpErrors.NotFound($"文章 {id} 不存在") : Results.Ok(post);
    }

    private static async Task<IResult> DeletePost(string id, ContentOperations operations, CancellationToken cancellationToken) {
        var deleted = await operations.DeletePostAsync(id, cancellationToken);
        return deleted ? Results.NoContent() : HttpErrors.NotFound($"文章 {id} 不存在");
    }

    private static async Task<IResult> UploadImage(string id, HttpRequest request, ContentOperations operations, CancellationToken cancellationToken) {
        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.FirstOrDefault();
        if (file == null) return HttpErrors.BadRequest("请上传图片文件");
        await using var stream = file.OpenReadStream();
        var url = await operations.UploadImageAsync(id, stream, file.FileName, cancellationToken);
        return url == null ? HttpErrors.NotFound($"文章 {id} 不存在") : Results.Ok(new { url });
    }

    private static async Task<IResult> FeaturePost(string id, ContentOperations operations, CancellationToken cancellationToken) {
        var featured = await operations.AddFeaturedPostAsync(id, cancellationToken);
        return featured == null ? HttpErrors.NotFound($"文章 {id} 不存在") : Results.Ok(new { featured.Id, featured.PostId });
    }

    private static async Task<IResult> TopPost(string id, ContentOperations operations, CancellationToken cancellationToken) {
        var top = await operations.SetTopPostAsync(id, cancellationToken);
        return top == null ? HttpErrors.NotFound($"文章 {id} 不存在") : Results.Ok(new { top.Id, top.PostId });
    }

    private static async Task<IResult> Translate(string id, TranslationOperations operations, [FromQuery] string language = "en", CancellationToken cancellationToken = default) {
        var (translation, error) = await operations.TranslateAsync(id, language, cancellationToken);
        if (error != null) return HttpErrors.BadRequest(error);
        return translation == null ? HttpErrors.NotFound($"文章 {id} 不存在") : Results.Ok(translation);
    }

    private static Task<PageResult<CategoryResponse>> ListCategories(
        ContentOperations operations,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        operations.GetCategoriesAsync(page, pageSize, cancellationToken);

    private static Task<IReadOnlyList<CategoryNodeResponse>?> CategoryTree(ContentOperations operations, CancellationToken cancellationToken) =>
        operations.GetCategoryTreeAsync(cancellationToken);

    private static Task<CategoryResponse> CreateCategory([FromBody] CategoryUpsertRequest request, ContentOperations operations, CancellationToken cancellationToken) =>
        operations.CreateCategoryAsync(request, cancellationToken);

    private static async Task<IResult> UpdateCategory(int id, [FromBody] CategoryUpsertRequest request, ContentOperations operations, CancellationToken cancellationToken) {
        var category = await operations.UpdateCategoryAsync(id, request, cancellationToken);
        return category == null ? HttpErrors.NotFound($"分类 {id} 不存在") : Results.Ok(category);
    }

    private static async Task<IResult> DeleteCategory(int id, ContentOperations operations, CancellationToken cancellationToken) {
        var deleted = await operations.DeleteCategoryAsync(id, cancellationToken);
        return deleted ? Results.NoContent() : HttpErrors.NotFound($"分类 {id} 不存在");
    }

    private static async Task<IResult> FeatureCategory(int id, [FromBody] FeaturedCategoryCreateRequest request, ContentOperations operations, CancellationToken cancellationToken) {
        var featured = await operations.AddFeaturedCategoryAsync(id, request, cancellationToken);
        return featured == null ? HttpErrors.NotFound($"分类 {id} 不存在") : Results.Ok(featured);
    }

    private static Task<BlogOverviewResponse> Overview(ContentOperations operations, CancellationToken cancellationToken) =>
        operations.OverviewAsync(cancellationToken);

    private static Task<IReadOnlyList<PublicationChannelResponse>> ListChannels(PublicationOperations operations, CancellationToken cancellationToken) =>
        operations.GetChannelsAsync(cancellationToken);

    private static Task<PublicationChannelResponse> SaveChannel([FromBody] PublicationChannelUpsertRequest request, PublicationOperations operations, CancellationToken cancellationToken) =>
        operations.SaveChannelAsync(null, request, cancellationToken);

    private static async Task<IResult> PreparePublication(string postId, [FromBody] PreparePublicationRequest request, PublicationOperations operations, CancellationToken cancellationToken) {
        var publication = await operations.PrepareAsync(postId, request, cancellationToken);
        return publication == null ? HttpErrors.BadRequest("文章或渠道不存在，或渠道已停用") : Results.Ok(publication);
    }

    private static async Task<IResult> DeliverPublication(string id, PublicationOperations operations, CancellationToken cancellationToken) {
        var publication = await operations.PublishAsync(id, cancellationToken);
        return publication == null ? HttpErrors.NotFound($"发布记录 {id} 不存在") : Results.Ok(publication);
    }
}
