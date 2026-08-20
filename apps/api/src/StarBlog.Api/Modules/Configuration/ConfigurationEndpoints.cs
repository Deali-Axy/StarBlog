using Microsoft.AspNetCore.Mvc;
using StarBlog.Api.Shared;

namespace StarBlog.Api.Modules.Configuration;

/// <summary>
/// 管理端站点配置 HTTP 边界。
/// </summary>
public static class ConfigurationEndpoints {
    /// <summary>映射 /api/v1/admin/settings，按 key 标识资源。</summary>
    public static IEndpointRouteBuilder MapConfigurationEndpoints(this IEndpointRouteBuilder endpoints) {
        var group = endpoints.MapGroup("/api/v1/admin/settings")
            .RequireAuthorization()
            .WithTags("admin")
            .WithGroupName("admin");

        group.MapGet("/", ListAsync);
        group.MapGet("/{key}", GetAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{key}", UpdateAsync);
        group.MapDelete("/{key}", DeleteAsync);
        return endpoints;
    }

    private static async Task<IReadOnlyList<ConfigItemResponse>> ListAsync(
        ConfigurationOperations operations,
        CancellationToken cancellationToken) =>
        await operations.GetAllAsync(cancellationToken);

    private static async Task<IResult> GetAsync(
        string key,
        ConfigurationOperations operations,
        CancellationToken cancellationToken) {
        var item = await operations.GetItemAsync(key, cancellationToken);
        return item == null ? HttpErrors.NotFound($"配置 {key} 不存在") : Results.Ok(item);
    }

    private static async Task<ConfigItemResponse> CreateAsync(
        [FromBody] ConfigItemUpsertRequest request,
        ConfigurationOperations operations,
        CancellationToken cancellationToken) =>
        await operations.UpsertAsync(request, cancellationToken);

    private static async Task<IResult> UpdateAsync(
        string key,
        [FromBody] ConfigItemUpsertRequest request,
        ConfigurationOperations operations,
        CancellationToken cancellationToken) {
        var payload = new ConfigItemUpsertRequest {
            Key = key,
            Value = request.Value,
            Description = request.Description
        };
        return Results.Ok(await operations.UpsertAsync(payload, cancellationToken));
    }

    private static async Task<IResult> DeleteAsync(
        string key,
        ConfigurationOperations operations,
        CancellationToken cancellationToken) {
        var deleted = await operations.DeleteAsync(key, cancellationToken);
        return deleted ? Results.NoContent() : HttpErrors.NotFound($"配置 {key} 不存在");
    }
}
