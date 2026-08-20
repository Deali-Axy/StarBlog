using Microsoft.EntityFrameworkCore;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Modules.Analytics.Domain;
using StarBlog.Api.Shared;

namespace StarBlog.Api.Modules.Analytics;

/// <summary>访问总览。</summary>
public sealed class VisitOverviewResponse {
    public int Total { get; init; }
    public int Pv { get; init; }
    public int Uv { get; init; }
    public int Api { get; init; }
    public int Spider { get; init; }
}

/// <summary>按天趋势。</summary>
public sealed class DailyTrendResponse {
    public DateTime Date { get; init; }
    public int Pv { get; init; }
    public int Uv { get; init; }
}

/// <summary>
/// 访问记录查询与统计。
/// </summary>
public sealed class AnalyticsOperations {
    private readonly StarBlogDbContext _db;

    public AnalyticsOperations(StarBlogDbContext db) {
        _db = db;
    }

    public async Task<PageResult<VisitRecord>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken) {
        page = Paging.NormalizePage(page);
        pageSize = Paging.NormalizePageSize(pageSize);
        var query = _db.VisitRecords.AsNoTracking().OrderByDescending(record => record.Time);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PageResult<VisitRecord> {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<VisitOverviewResponse> OverviewAsync(CancellationToken cancellationToken) {
        var records = await _db.VisitRecords.AsNoTracking().ToListAsync(cancellationToken);
        return new VisitOverviewResponse {
            Total = records.Count,
            Pv = records.Count(record => !record.RequestPath.StartsWith("/api", StringComparison.OrdinalIgnoreCase) && !record.UserAgentInfo.Device.IsSpider),
            Uv = records.Where(record => !record.RequestPath.StartsWith("/api", StringComparison.OrdinalIgnoreCase) && !record.UserAgentInfo.Device.IsSpider)
                .Select(record => record.Ip).Distinct().Count(),
            Api = records.Count(record => record.RequestPath.StartsWith("/api", StringComparison.OrdinalIgnoreCase)),
            Spider = records.Count(record => record.UserAgentInfo.Device.IsSpider)
        };
    }

    public async Task<IReadOnlyList<DailyTrendResponse>> DailyTrendAsync(int days, CancellationToken cancellationToken) {
        var start = DateTime.Today.AddDays(-days);
        var records = await _db.VisitRecords.AsNoTracking()
            .Where(record => record.Time >= start)
            .Select(record => new { record.Time.Date, record.Ip, record.RequestPath, IsSpider = record.UserAgentInfo.Device.IsSpider })
            .ToListAsync(cancellationToken);
        return records
            .Where(record => !record.RequestPath.StartsWith("/api", StringComparison.OrdinalIgnoreCase) && !record.IsSpider)
            .GroupBy(record => record.Date)
            .OrderBy(group => group.Key)
            .Select(group => new DailyTrendResponse {
                Date = group.Key,
                Pv = group.Count(),
                Uv = group.Select(item => item.Ip).Distinct().Count()
            })
            .ToList();
    }

    public object RuntimeStatistics() {
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        return new {
            machineName = Environment.MachineName,
            workingSet = process.WorkingSet64,
            gcMemory = GC.GetTotalMemory(false),
            threadCount = process.Threads.Count,
            startedAt = process.StartTime
        };
    }
}
