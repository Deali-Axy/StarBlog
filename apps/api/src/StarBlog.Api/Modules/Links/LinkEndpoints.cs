using Microsoft.AspNetCore.Mvc;
using StarBlog.Api.Shared;

namespace StarBlog.Api.Modules.Links;

/// <summary>
/// 公开友链、管理端 CRUD、申请与审核。
/// </summary>
public static class LinkEndpoints {
    public static IEndpointRouteBuilder MapLinkEndpoints(this IEndpointRouteBuilder endpoints) {
        endpoints.MapGet("/api/v1/links", GetPublic)
            .AllowAnonymous()
            .WithTags("link")
            .WithGroupName("link");

        var admin = endpoints.MapGroup("/api/v1/admin/links")
            .RequireAuthorization()
            .WithTags("link")
            .WithGroupName("link");
        admin.MapGet("/", GetAll);
        admin.MapGet("/{id:int}", GetById);
        admin.MapPost("/", Create);
        admin.MapPut("/{id:int}", Update);
        admin.MapDelete("/{id:int}", Delete);

        endpoints.MapPost("/api/v1/site/link-exchanges", Apply)
            .AllowAnonymous()
            .WithTags("link")
            .WithGroupName("link");

        var requests = endpoints.MapGroup("/api/v1/admin/link-exchange-requests")
            .RequireAuthorization()
            .WithTags("link")
            .WithGroupName("link");
        requests.MapGet("/", GetExchanges);
        requests.MapGet("/{id:int}", GetExchange);
        requests.MapPatch("/{id:int}/approval", Accept);
        requests.MapPatch("/{id:int}/rejection", Reject);
        requests.MapDelete("/{id:int}", DeleteExchange);
        return endpoints;
    }

    private static Task<IReadOnlyList<LinkResponse>> GetPublic(LinkOperations operations, CancellationToken cancellationToken) =>
        operations.GetVisibleAsync(cancellationToken);

    private static Task<IReadOnlyList<LinkResponse>> GetAll(LinkOperations operations, CancellationToken cancellationToken) =>
        operations.GetAllAsync(cancellationToken);

    private static async Task<IResult> GetById(int id, LinkOperations operations, CancellationToken cancellationToken) {
        var link = await operations.GetByIdAsync(id, cancellationToken);
        return link == null ? HttpErrors.NotFound($"友链 {id} 不存在") : Results.Ok(link);
    }

    private static Task<LinkResponse> Create([FromBody] LinkUpsertRequest request, LinkOperations operations, CancellationToken cancellationToken) =>
        operations.CreateAsync(request, cancellationToken);

    private static async Task<IResult> Update(int id, [FromBody] LinkUpsertRequest request, LinkOperations operations, CancellationToken cancellationToken) {
        var link = await operations.UpdateAsync(id, request, cancellationToken);
        return link == null ? HttpErrors.NotFound($"友链 {id} 不存在") : Results.Ok(link);
    }

    private static async Task<IResult> Delete(int id, LinkOperations operations, CancellationToken cancellationToken) {
        var deleted = await operations.DeleteAsync(id, cancellationToken);
        return deleted ? Results.NoContent() : HttpErrors.NotFound($"友链 {id} 不存在");
    }

    private static async Task<IResult> Apply([FromBody] LinkExchangeApplicationRequest request, LinkOperations operations, CancellationToken cancellationToken) {
        var (response, error) = await operations.ApplyAsync(request, cancellationToken);
        return error != null ? HttpErrors.BadRequest(error) : Results.Ok(response);
    }

    private static Task<IReadOnlyList<LinkExchangeResponse>> GetExchanges(LinkOperations operations, CancellationToken cancellationToken) =>
        operations.GetExchangesAsync(cancellationToken);

    private static async Task<IResult> GetExchange(int id, LinkOperations operations, CancellationToken cancellationToken) {
        var item = await operations.GetExchangeAsync(id, cancellationToken);
        return item == null ? HttpErrors.NotFound($"申请 {id} 不存在") : Results.Ok(item);
    }

    private static async Task<IResult> Accept(int id, [FromBody] LinkExchangeDecisionRequest? request, LinkOperations operations, CancellationToken cancellationToken) {
        var item = await operations.DecideAsync(id, true, request?.Reason, cancellationToken);
        return item == null ? HttpErrors.NotFound($"申请 {id} 不存在") : Results.Ok(item);
    }

    private static async Task<IResult> Reject(int id, [FromBody] LinkExchangeDecisionRequest? request, LinkOperations operations, CancellationToken cancellationToken) {
        var item = await operations.DecideAsync(id, false, request?.Reason, cancellationToken);
        return item == null ? HttpErrors.NotFound($"申请 {id} 不存在") : Results.Ok(item);
    }

    private static async Task<IResult> DeleteExchange(int id, LinkOperations operations, CancellationToken cancellationToken) {
        var deleted = await operations.DeleteExchangeAsync(id, cancellationToken);
        return deleted ? Results.NoContent() : HttpErrors.NotFound($"申请 {id} 不存在");
    }
}
