using Microsoft.AspNetCore.Mvc;
using StarBlog.Api.Shared;

namespace StarBlog.Api.Modules.Analytics;

/// <summary>
/// 访问记录与运行时统计。
/// </summary>
public static class AnalyticsEndpoints {
    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder endpoints) {
        var records = endpoints.MapGroup("/api/v1/admin/visit-records")
            .RequireAuthorization()
            .WithTags("admin")
            .WithGroupName("admin");
        records.MapGet("/", ListAsync);
        records.MapGet("/reports/overview", OverviewAsync);
        records.MapGet("/reports/daily-trends", DailyTrendAsync);

        endpoints.MapGet("/api/v1/admin/runtime/statistics", RuntimeAsync)
            .RequireAuthorization()
            .WithTags("admin")
            .WithGroupName("admin");
        return endpoints;
    }

    private static Task<PageResult<VisitRecordResponse>> ListAsync(
        AnalyticsOperations operations,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        operations.GetPagedAsync(page, pageSize, cancellationToken);

    private static Task<VisitOverviewResponse> OverviewAsync(AnalyticsOperations operations, CancellationToken cancellationToken) =>
        operations.OverviewAsync(cancellationToken);

    private static Task<IReadOnlyList<DailyTrendResponse>> DailyTrendAsync(
        AnalyticsOperations operations,
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default) =>
        operations.DailyTrendAsync(days, cancellationToken);

    private static RuntimeStatisticsResponse RuntimeAsync(AnalyticsOperations operations) =>
        operations.RuntimeStatistics();
}
