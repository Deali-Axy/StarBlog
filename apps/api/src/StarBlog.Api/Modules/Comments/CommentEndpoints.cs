using Microsoft.AspNetCore.Mvc;
using StarBlog.Api.Shared;

namespace StarBlog.Api.Modules.Comments;

/// <summary>
/// 公开评论、OTP 与管理端审核。
/// </summary>
public static class CommentEndpoints {
    public static IEndpointRouteBuilder MapCommentEndpoints(this IEndpointRouteBuilder endpoints) {
        var comments = endpoints.MapGroup("/api/v1/comments").WithTags("comment").WithGroupName("comment");
        comments.MapGet("/", ListPublic);
        comments.MapGet("/by-post/{postId}", Tree);
        comments.MapPost("/email-otp", SendOtp);
        comments.MapPost("/", Create);

        var admin = endpoints.MapGroup("/api/v1/admin/comments")
            .RequireAuthorization()
            .WithTags("comment")
            .WithGroupName("comment");
        admin.MapGet("/", ListAdmin);
        admin.MapPatch("/{id}/approval", Accept);
        admin.MapPatch("/{id}/rejection", Reject);
        return endpoints;
    }

    private static Task<PageResult<CommentResponse>> ListPublic(
        CommentOperations operations,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? postId = null,
        CancellationToken cancellationToken = default) =>
        operations.GetPagedAsync(page, pageSize, postId, adminMode: false, cancellationToken);

    private static Task<PageResult<CommentResponse>> ListAdmin(
        CommentOperations operations,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? postId = null,
        CancellationToken cancellationToken = default) =>
        operations.GetPagedAsync(page, pageSize, postId, adminMode: true, cancellationToken);

    private static Task<IReadOnlyList<CommentResponse>> Tree(string postId, CommentOperations operations, CancellationToken cancellationToken) =>
        operations.GetTreeAsync(postId, cancellationToken);

    private static async Task<IResult> SendOtp([FromQuery] string email, CommentOperations operations, CancellationToken cancellationToken) {
        if (!CommentOperations.IsValidEmail(email)) return HttpErrors.BadRequest("提供的邮箱地址无效");
        var (sent, _, message) = await operations.GenerateOtpAsync(email, cancellationToken);
        return sent ? Results.Ok(new { message }) : HttpErrors.BadRequest(message);
    }

    private static async Task<IResult> Create(
        [FromBody] CommentCreateRequest request,
        HttpContext context,
        CommentOperations operations,
        CancellationToken cancellationToken) {
        if (!operations.VerifyOtp(request.Email, request.EmailOtp)) return HttpErrors.BadRequest("验证码无效");
        var ip = context.Connection.RemoteIpAddress?.ToString();
        var (comment, message) = await operations.AddAsync(request, ip, context.Request.Headers.UserAgent, cancellationToken);
        return Results.Ok(new { comment, message });
    }

    private static async Task<IResult> Accept(string id, [FromBody] CommentDecisionRequest? request, CommentOperations operations, CancellationToken cancellationToken) {
        var comment = await operations.AcceptAsync(id, request?.Reason, cancellationToken);
        return comment == null ? HttpErrors.NotFound($"评论 {id} 不存在") : Results.Ok(comment);
    }

    private static async Task<IResult> Reject(string id, [FromBody] CommentDecisionRequest? request, CommentOperations operations, CancellationToken cancellationToken) {
        var comment = await operations.RejectAsync(id, request?.Reason, cancellationToken);
        return comment == null ? HttpErrors.NotFound($"评论 {id} 不存在") : Results.Ok(comment);
    }
}
