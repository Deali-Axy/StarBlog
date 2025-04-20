using System.Linq.Dynamic.Core;
using IP2Region.Net.Abstractions;
using Microsoft.EntityFrameworkCore;
using StarBlog.Data;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels.QueryFilters;
using StarBlog.Web.ViewModels.VisitRecord;
using X.PagedList;

namespace StarBlog.Web.Services;

public class VisitRecordService {
    private readonly ILogger<VisitRecordService> _logger;
    private readonly AppDbContext _dbContext;
    private readonly ISearcher _searcher;

    public VisitRecordService(ILogger<VisitRecordService> logger, AppDbContext dbContext, ISearcher searcher) {
        _logger = logger;
        _dbContext = dbContext;
        _searcher = searcher;
    }

    private IQueryable<VisitRecord> GetQuerySet(VisitRecordFilter? filter = null) {
        var qs = _dbContext.VisitRecords.AsQueryable();
        if (filter == null) {
            return qs;
        }

        if (filter.ExcludeApi) {
            qs = qs.Where(e => !e.RequestPath.ToLower().StartsWith("/api"));
        }

        if (filter.Date.HasValue) {
            qs = qs.Where(e => e.Time.Date == filter.Date.Value.Date);
        }

        if (filter.From.HasValue) {
            qs = qs.Where(e => e.Time >= filter.From.Value);
        }

        if (filter.To.HasValue) {
            qs = qs.Where(e => e.Time < filter.To.Value);
        }

        return qs;
    }

    public async Task<VisitRecord?> GetById(int id) {
        var item = await _dbContext.VisitRecords.FirstOrDefaultAsync(e => e.Id == id);
        return item;
    }

    public async Task<List<VisitRecord>> GetAll(VisitRecordFilter? filter = null) {
        return await _dbContext.VisitRecords.OrderByDescending(e => e.Time).ToListAsync();
    }

    public async Task<IPagedList<VisitRecord>> GetPagedList(VisitRecordQueryParameters param) {
        var qs = GetQuerySet();

        // 搜索
        if (!string.IsNullOrEmpty(param.Search)) {
            qs = qs.Where(a => a.RequestPath.Contains(param.Search));
        }

        // 排序
        if (!string.IsNullOrEmpty(param.SortBy)) {
            var isDesc = param.SortBy.StartsWith("-");
            var orderByProperty = param.SortBy.Trim('-');
            if (isDesc) {
                orderByProperty = $"{orderByProperty} desc";
            }

            qs = qs.OrderBy(orderByProperty);
        }

        // 筛选
        if (!string.IsNullOrWhiteSpace(param.Country)) {
            qs = qs.Where(e => e.Country != null && e.Country.Contains(param.Country));
        }

        if (!string.IsNullOrWhiteSpace(param.Province)) {
            qs = qs.Where(e => e.Province != null && e.Province.Contains(param.Province));
        }

        if (!string.IsNullOrWhiteSpace(param.City)) {
            qs = qs.Where(e => e.City != null && e.City.Contains(param.City));
        }

        if (!string.IsNullOrWhiteSpace(param.Isp)) {
            qs = qs.Where(e => e.Isp != null && e.Isp.Contains(param.Isp));
        }

        IPagedList<VisitRecord> pagedList = new StaticPagedList<VisitRecord>(
            await qs.Page(param.Page, param.PageSize).ToListAsync(),
            param.Page, param.PageSize,
            Convert.ToInt32(await qs.CountAsync())
        );
        return pagedList;
    }

    /// <summary>
    /// 总览数据
    /// </summary>
    /// <returns></returns>
    public async Task<object> Overview(VisitRecordFilter? filter = null) {
        filter ??= new VisitRecordFilter { ExcludeApi = true };
        var querySet = GetQuerySet(filter);

        return new {
            TotalVisit = await querySet.CountAsync(),
            TodayVisit = await querySet.Where(e => e.Time.Date == DateTime.Today).CountAsync(),
            YesterdayVisit = await querySet
                .Where(e => e.Time.Date == DateTime.Today.AddDays(-1).Date)
                .CountAsync()
        };
    }

    /// <summary>
    /// 趋势数据
    /// </summary>
    /// <param name="days">查看最近几天的数据，默认7天</param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<object> Trend(int days = 7, VisitRecordFilter? filter = null) {
        filter ??= new VisitRecordFilter { ExcludeApi = true };
        var qs = GetQuerySet(filter);
        var startDate = DateTime.Today.AddDays(-days).Date;
        return qs.Where(e => e.Time.Date >= startDate)
            .GroupBy(e => e.Time.Date)
            .Select(g => new {
                time = g.Key,
                date = $"{g.Key.Month}-{g.Key.Day}",
                count = g.Count()
            })
            .OrderBy(e => e.time)
            .ToListAsync();
    }

    /// <summary>
    /// 统计数据
    /// </summary>
    public async Task<object> Stats(DateTime date, VisitRecordFilter? filter = null) {
        filter ??= new VisitRecordFilter { ExcludeApi = true };
        var qs = GetQuerySet(filter);
        return new {
            Count = qs.Where(e => e.Time.Date == date).CountAsync()
        };
    }

    /// <summary>
    /// 获取地理信息筛选参数
    /// </summary>
    public async Task<List<string?>> GetGeoFilterParams(string param = "country", 
        string? country = null, string? province = null, string? city = null) {
        var qs = GetQuerySet();
        // 筛选
        if (!string.IsNullOrWhiteSpace(country)) {
            qs = qs.Where(e => e.Country != null && e.Country.Contains(country));
        }

        if (!string.IsNullOrWhiteSpace(province)) {
            qs = qs.Where(e => e.Province != null && e.Province.Contains(province));
        }

        if (!string.IsNullOrWhiteSpace(city)) {
            qs = qs.Where(e => e.City != null && e.City.Contains(city));
        }

        switch (param) {
            case "country":
                return await qs.Select(e => e.Country).Distinct().ToListAsync();
            case "province":
                return await qs.Select(e => e.Province).Distinct().ToListAsync();
            case "city":
                return await qs.Select(e => e.City).Distinct().ToListAsync();
            default:
                return new List<string?>();
        }
    
    }

    /// <summary>
    /// PV / UV 统计
    /// <para>PV（Page Views）：总访问数</para>
    /// <para>UV（Unique Visitors）：去重的 IP 数或去重的用户数</para>
    /// </summary>
    public async Task<PvUv> GetPvUv(DateTime date, VisitRecordFilter? filter = null) {
        filter ??= new VisitRecordFilter { ExcludeApi = true };
        var qs = GetQuerySet(filter);
        qs = qs.Where(e => e.Time.Date == date.Date);
        var pv = await qs.CountAsync();
        var uv = await qs.Select(e => e.Ip).Distinct().CountAsync();
        return new PvUv { Date = date.Date, PV = pv, UV = uv };
    }

    /// <summary>
    /// Top N 访问路径
    /// <para>找出某天或某段时间最热的页面／接口</para>
    /// </summary>
    public async Task<List<TopPath>> GetTopPaths(DateTime from, DateTime to, int top = 10,
        VisitRecordFilter? filter = null
    ) {
        filter ??= new VisitRecordFilter { ExcludeApi = true };
        var qs = GetQuerySet(filter);
        return await qs.Where(e => e.Time >= from && e.Time < to)
            .GroupBy(e => e.RequestPath)
            .Select(g => new TopPath { Path = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync();
    }

    /// <summary>
    /// 响应时间分布
    /// <para>计算平均、最大、95 百分位等</para>
    /// </summary>
    public async Task<object> GetResponseTimeStats(DateTime date, VisitRecordFilter? filter = null) {
        filter ??= new VisitRecordFilter { ExcludeApi = true };
        var q = GetQuerySet(filter);
        q = q.Where(e => e.Time.Date == date.Date);
        var avg = await q.AverageAsync(e => e.ResponseTimeMs);
        var max = await q.MaxAsync(e => e.ResponseTimeMs);
        var p95 = await q
            .OrderBy(e => e.ResponseTimeMs)
            .Skip((int)(await q.CountAsync() * 0.95))
            .Select(e => e.ResponseTimeMs)
            .FirstOrDefaultAsync();
        return new { Date = date.Date, Avg = avg, Max = max, P95 = p95 };
    }

    /// <summary>
    /// 状态码分布
    /// <para>统计各状态码的调用量</para>
    /// </summary>
    public async Task<List<StatusCodeDistribution>> GetStatusCodeDistribution(VisitRecordFilter? filter = null) {
        filter ??= new VisitRecordFilter { ExcludeApi = true };
        var qs = GetQuerySet(filter);
        return await qs
            .GroupBy(e => e.StatusCode)
            .Select(g => new StatusCodeDistribution { Code = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
    }

    /// <summary>
    /// 小时／分钟级趋势
    /// <para>按小时查看访问量变化</para>
    /// </summary>
    public async Task<List<HourlyTrend>> GetHourlyTrend(VisitRecordFilter? filter = null) {
        filter ??= new VisitRecordFilter { ExcludeApi = true };
        var qs = GetQuerySet(filter);
        return await qs.GroupBy(e => e.Time.Hour)
            .Select(g => new HourlyTrend { Hour = g.Key, Count = g.Count() })
            .OrderBy(x => x.Hour)
            .ToListAsync();
    }

    /// <summary>
    /// 地理位置分布统计
    /// </summary>
    public async Task<object> GetGeoDistribution(VisitRecordFilter? filter = null) {
        filter ??= new VisitRecordFilter { ExcludeApi = true };
        var query = GetQuerySet(filter);

        var countryStats = await query.GroupBy(e => e.Country)
            .Select(g => new { Country = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        var provinceStats = await query.GroupBy(e => e.Province)
            .Select(g => new { Province = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        var cityStats = await query.GroupBy(e => e.City)
            .Select(g => new { City = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        var ispStats = await query.GroupBy(e => e.Isp)
            .Select(g => new { Isp = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return new {
            Country = countryStats,
            Province = provinceStats,
            City = cityStats,
            Isp = ispStats
        };
    }
}