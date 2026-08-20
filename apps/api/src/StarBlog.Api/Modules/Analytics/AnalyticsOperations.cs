using Microsoft.EntityFrameworkCore;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Modules.Analytics.Domain;
using StarBlog.Api.Shared;

namespace StarBlog.Api.Modules.Analytics;

/// <summary>
/// 访问记录查询与统计。查询结果映射为契约，不把实体送出模块边界。
/// </summary>
public sealed class AnalyticsOperations {
    private readonly StarBlogDbContext _db;

    public AnalyticsOperations(StarBlogDbContext db) {
        _db = db;
    }

    /// <summary>分页返回访问记录契约，按时间倒序。</summary>
    public async Task<PageResult<VisitRecordResponse>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken) {
        page = Paging.NormalizePage(page);
        pageSize = Paging.NormalizePageSize(pageSize);
        var query = _db.VisitRecords.AsNoTracking().OrderByDescending(record => record.Time);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PageResult<VisitRecordResponse> {
            Items = items.Select(ToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    /// <summary>计算 PV/UV、接口访问和爬虫占比。</summary>
    public async Task<VisitOverviewResponse> OverviewAsync(CancellationToken cancellationToken) {
        var records = await _db.VisitRecords.AsNoTracking().ToListAsync(cancellationToken);
        return new VisitOverviewResponse {
            Total = records.Count,
            Pv = records.Count(IsHumanPageView),
            Uv = records.Where(IsHumanPageView).Select(record => record.Ip).Distinct().Count(),
            Api = records.Count(record => IsApiPath(record.RequestPath)),
            Spider = records.Count(record => record.UserAgentInfo.Device.IsSpider)
        };
    }

    /// <summary>按自然日聚合最近若干天的 PV/UV。</summary>
    public async Task<IReadOnlyList<DailyTrendResponse>> DailyTrendAsync(int days, CancellationToken cancellationToken) {
        var start = DateTime.Today.AddDays(-Math.Clamp(days, 1, 366));
        var records = await _db.VisitRecords.AsNoTracking()
            .Where(record => record.Time >= start)
            .Select(record => new { record.Time.Date, record.Ip, record.RequestPath, IsSpider = record.UserAgentInfo.Device.IsSpider })
            .ToListAsync(cancellationToken);
        return records
            .Where(record => !IsApiPath(record.RequestPath) && !record.IsSpider)
            .GroupBy(record => record.Date)
            .OrderBy(group => group.Key)
            .Select(group => new DailyTrendResponse {
                Date = group.Key,
                Pv = group.Count(),
                Uv = group.Select(item => item.Ip).Distinct().Count()
            })
            .ToList();
    }

    /// <summary>读取当前进程的内存和线程快照，供管理端监控。</summary>
    public RuntimeStatisticsResponse RuntimeStatistics() {
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        return new RuntimeStatisticsResponse {
            MachineName = Environment.MachineName,
            WorkingSet = process.WorkingSet64,
            GcMemory = GC.GetTotalMemory(false),
            ThreadCount = process.Threads.Count,
            StartedAt = process.StartTime
        };
    }

    /// <summary>把访问记录实体压成扁平契约。</summary>
    private static VisitRecordResponse ToResponse(VisitRecord record) => new() {
        Id = record.Id,
        Ip = record.Ip,
        Country = record.IpInfo.Country,
        Province = record.IpInfo.Province,
        City = record.IpInfo.City,
        Isp = record.IpInfo.Isp,
        RequestPath = record.RequestPath,
        RequestQueryString = record.RequestQueryString,
        RequestMethod = record.RequestMethod,
        UserAgent = record.UserAgent,
        OsFamily = record.UserAgentInfo.OS.Family,
        DeviceFamily = record.UserAgentInfo.Device.Family,
        BrowserFamily = record.UserAgentInfo.UserAgent.Family,
        IsSpider = record.UserAgentInfo.Device.IsSpider,
        Time = record.Time,
        StatusCode = record.StatusCode,
        ResponseTimeMs = record.ResponseTimeMs,
        Referrer = record.Referrer
    };

    private static bool IsApiPath(string path) =>
        path.StartsWith("/api", StringComparison.OrdinalIgnoreCase);

    private static bool IsHumanPageView(VisitRecord record) =>
        !IsApiPath(record.RequestPath) && !record.UserAgentInfo.Device.IsSpider;
}
