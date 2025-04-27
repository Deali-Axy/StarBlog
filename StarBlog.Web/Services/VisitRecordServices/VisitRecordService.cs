using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using StarBlog.Data;
using StarBlog.Data.Models;
using StarBlog.Web.Criteria;
using StarBlog.Web.Extensions;
using StarBlog.Web.ViewModels.VisitRecord;
using X.PagedList;

namespace StarBlog.Web.Services.VisitRecordServices;

public class VisitRecordService {
    private readonly ILogger<VisitRecordService> _logger;
    private readonly AppDbContext _dbContext;

    public VisitRecordService(ILogger<VisitRecordService> logger, AppDbContext dbContext) {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<VisitRecord?> GetById(int id) {
        var item = await _dbContext.VisitRecords.FirstOrDefaultAsync(e => e.Id == id);
        return item;
    }

    public async Task<List<VisitRecord>> GetAll(VisitRecordParameters p) {
        return await _dbContext.VisitRecords.ApplyFilters(p).ToListAsync();
    }

    public Task<IPagedList<VisitRecord>> GetPagedList(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        return qs.ToPagedListAsync(p);
    }

    /// <summary>
    /// 总览数据
    /// </summary>
    /// <returns></returns>
    public async Task<object> Overview(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);

        return new {
            TotalVisit = await qs.CountAsync(),
            TodayVisit = await qs.Where(e => e.Time.Date == DateTime.Today).CountAsync(),
            YesterdayVisit = await qs
                .Where(e => e.Time.Date == DateTime.Today.AddDays(-1).Date)
                .CountAsync()
        };
    }

    /// <summary>
    /// 获取按天趋势数据
    /// </summary>
    /// <param name="p"></param>
    /// <param name="days">查看最近几天的数据，默认7天</param>
    /// <returns></returns>
    public async Task<object> GetDailyTrend(VisitRecordParameters p, int days) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        var startDate = DateTime.Today.AddDays(-days).Date;

        var dailyStats = await qs.Where(e => e.Time.Date >= startDate)
            .GroupBy(e => e.Time.Date)
            .Select(g => new {
                time = g.Key,
                date = $"{g.Key.Month}-{g.Key.Day}",
                // 总访问量
                total = g.Count(),
                // page view (exclude api and spiders)
                pv = g.Count(e =>
                    !e.RequestPath.ToLower().StartsWith("/api") &&
                    !e.UserAgentInfo.Device.IsSpider
                ),
                // unique visitors
                uv = g.Where(e =>
                        !e.RequestPath.ToLower().StartsWith("/api") &&
                        !e.UserAgentInfo.Device.IsSpider)
                    .Select(e => e.Ip).Distinct().Count(),
                // api visit count
                api = g.Count(e => e.RequestPath.ToLower().StartsWith("/api")),
                spider = g.Count(e => e.UserAgentInfo.Device.IsSpider),
            })
            .OrderBy(e => e.time)
            .ToListAsync();

        return dailyStats;
    }

    /// <summary>
    /// 小时／分钟级趋势
    /// <para>按小时查看访问量变化</para>
    /// </summary>
    public async Task<List<HourlyTrend>> GetHourlyTrend(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        return await qs.GroupBy(e => e.Time.Hour)
            .Select(g => new HourlyTrend {
                Hour = g.Key,
                // 总访问量
                Total = g.Count(),
                // page view (exclude api and spiders)
                Pv = g.Count(e =>
                    !e.RequestPath.ToLower().StartsWith("/api") &&
                    !e.UserAgentInfo.Device.IsSpider
                ),
                // unique visitors
                Uv = g.Where(e =>
                        !e.RequestPath.ToLower().StartsWith("/api") &&
                        !e.UserAgentInfo.Device.IsSpider)
                    .Select(e => e.Ip).Distinct().Count(),
                // api visit count
                Api = g.Count(e => e.RequestPath.ToLower().StartsWith("/api")),
                Spider = g.Count(e => e.UserAgentInfo.Device.IsSpider),
            })
            .OrderBy(e => e.Hour)
            .ToListAsync();
    }

    /// <summary>
    /// 获取地理信息筛选参数
    /// </summary>
    public async Task<List<string?>> GetGeoFilterParams(VisitRecordParameters p, string param = "country") {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        return param switch {
            "country" => await qs.Select(e => e.IpInfo.Country).Distinct().ToListAsync(),
            "province" => await qs.Select(e => e.IpInfo.Province).Distinct().ToListAsync(),
            "city" => await qs.Select(e => e.IpInfo.City).Distinct().ToListAsync(),
            _ => new List<string?>()
        };
    }

    public async Task<object> GetUserAgentFilterParams(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        return new {
            OS = await qs.Select(e => e.UserAgentInfo.OS.Family).Distinct().ToListAsync(),
            Device = await qs.Select(e => e.UserAgentInfo.Device.Family).Distinct().ToListAsync(),
            UserAgent = await qs.Select(e => e.UserAgentInfo.UserAgent.Family).Distinct().ToListAsync(),
        };
    }

    /// <summary>
    /// PV / UV 统计
    /// <para>PV（Page Views）：总访问数</para>
    /// <para>UV（Unique Visitors）：去重的 IP 数或去重的用户数</para>
    /// </summary>
    public async Task<PvUv> GetPvUv(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        var pv = await qs.CountAsync();
        var uv = await qs.Select(e => e.Ip).Distinct().CountAsync();
        return new PvUv { PV = pv, UV = uv };
    }

    /// <summary>
    /// Top N 访问路径
    /// <para>找出某天或某段时间最热的页面／接口</para>
    /// </summary>
    public async Task<List<TopPath>> GetTopPaths(VisitRecordParameters p, int top = 10) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        return await qs.GroupBy(e => e.RequestPath)
            .Select(g => new TopPath { Path = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync();
    }

    /// <summary>
    /// 响应时间分布
    /// <para>计算平均、最大、95 百分位等</para>
    /// </summary>
    public async Task<object> GetResponseTimeStats(VisitRecordParameters p) {
        var q = _dbContext.VisitRecords.ApplyFilters(p);
        var avg = await q.AverageAsync(e => e.ResponseTimeMs);
        var max = await q.MaxAsync(e => e.ResponseTimeMs);
        var p95 = await q
            .OrderBy(e => e.ResponseTimeMs)
            .Skip((int)(await q.CountAsync() * 0.95))
            .Select(e => e.ResponseTimeMs)
            .FirstOrDefaultAsync();
        return new { Avg = avg, Max = max, P95 = p95 };
    }

    /// <summary>
    /// 状态码分布
    /// <para>统计各状态码的调用量</para>
    /// </summary>
    public async Task<List<StatusCodeDistribution>> GetStatusCodeDistribution(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        return await qs
            .GroupBy(e => e.StatusCode)
            .Select(g => new StatusCodeDistribution { Code = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
    }

    /// <summary>
    /// 地理位置分布统计
    /// </summary>
    public async Task<object> GetGeoDistribution(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);

        var countryStats = await qs.GroupBy(e => e.IpInfo.Country)
            .Select(g => new { Country = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        var provinceStats = await qs.GroupBy(e => e.IpInfo.Province)
            .Select(g => new { Province = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        var cityStats = await qs.GroupBy(e => e.IpInfo.City)
            .Select(g => new { City = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        var ispStats = await qs.GroupBy(e => e.IpInfo.Isp)
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