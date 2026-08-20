using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using StarBlog.Data;
using StarBlog.Data.Models;
using StarBlog.Application.Criteria;
using StarBlog.Application.Extensions;
using StarBlog.Application.ViewModels.VisitRecord;
using X.PagedList;

namespace StarBlog.Application.Services.VisitRecordServices;

public class VisitRecordAnalyticsService {
    private readonly ILogger<VisitRecordAnalyticsService> _logger;
    private readonly AppDbContext _dbContext;

    public VisitRecordAnalyticsService(ILogger<VisitRecordAnalyticsService> logger, AppDbContext dbContext) {
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
    public async Task<VisitOverview> Overview(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);

        var data = new VisitOverview {
            Total = await qs.CountAsync(),
            Pv = qs.Count(e =>
                !e.RequestPath.ToLower().StartsWith("/api") &&
                !e.UserAgentInfo.Device.IsSpider
            ),
            Uv = qs.Where(e =>
                    !e.RequestPath.ToLower().StartsWith("/api") &&
                    !e.UserAgentInfo.Device.IsSpider)
                .Select(e => e.Ip).Distinct().Count(),
            Api = qs.Count(e => e.RequestPath.ToLower().StartsWith("/api")),
            Spider = qs.Count(e => e.UserAgentInfo.Device.IsSpider),
        };

        return data;
    }

    /// <summary>
    /// 获取按天趋势数据 - 使用拆分查询避免复杂 LINQ 翻译问题
    /// </summary>
    /// <param name="p"></param>
    /// <param name="days">查看最近几天的数据，默认7天</param>
    /// <returns></returns>
    public async Task<List<DailyTrend>> GetDailyTrend(VisitRecordParameters p, int days) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        var startDate = DateTime.Today.AddDays(-days).Date;

        // 获取基础数据到内存中进行处理
        var records = await qs.Where(e => e.Time.Date >= startDate)
            .Select(e => new {
                e.Time.Date,
                e.RequestPath,
                e.Ip,
                IsSpider = e.UserAgentInfo.Device.IsSpider
            })
            .ToListAsync();

        // 在内存中进行分组和统计
        var data = records
            .GroupBy(e => e.Date)
            .Select(g => new DailyTrend {
                Time = g.Key,
                Date = $"{g.Key.Month}-{g.Key.Day}",
                // 总访问量
                Total = g.Count(),
                // page view (exclude api and spiders)
                Pv = g.Count(e =>
                    !e.RequestPath.ToLower().StartsWith("/api") &&
                    !e.IsSpider
                ),
                // unique visitors
                Uv = g.Where(e =>
                        !e.RequestPath.ToLower().StartsWith("/api") &&
                        !e.IsSpider)
                    .Select(e => e.Ip).Distinct().Count(),
                // api visit count
                Api = g.Count(e => e.RequestPath.ToLower().StartsWith("/api")),
                Spider = g.Count(e => e.IsSpider),
            })
            .OrderBy(e => e.Time)
            .ToList();

        return data;
    }

    /// <summary>
    /// 小时级趋势 - 使用拆分查询避免复杂 LINQ 翻译问题
    /// <para>按小时查看访问量变化</para>
    /// </summary>
    public async Task<List<HourlyTrend>> GetHourlyTrend(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        
        // 获取基础数据到内存中进行处理
        var records = await qs
            .Select(e => new {
                e.Time.Hour,
                e.RequestPath,
                e.Ip,
                IsSpider = e.UserAgentInfo.Device.IsSpider
            })
            .ToListAsync();

        // 在内存中进行分组和统计
        var data = records
            .GroupBy(e => e.Hour)
            .Select(g => new HourlyTrend {
                Hour = g.Key,
                // 总访问量
                Total = g.Count(),
                // page view (exclude api and spiders)
                Pv = g.Count(e =>
                    !e.RequestPath.ToLower().StartsWith("/api") &&
                    !e.IsSpider
                ),
                // unique visitors
                Uv = g.Where(e =>
                        !e.RequestPath.ToLower().StartsWith("/api") &&
                        !e.IsSpider)
                    .Select(e => e.Ip).Distinct().Count(),
                // api visit count
                Api = g.Count(e => e.RequestPath.ToLower().StartsWith("/api")),
                Spider = g.Count(e => e.IsSpider),
            })
            .OrderBy(e => e.Hour)
            .ToList();

        return data;
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

    public async Task<UserAgentFilterParams> GetUserAgentFilterParams(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        return new UserAgentFilterParams {
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

    /// <summary>
    /// 来源域名分析
    /// <para>统计访问来源的域名分布</para>
    /// </summary>
    public async Task<List<ReferrerDomain>> GetReferrerDomains(VisitRecordParameters p, int top = 10) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        // 先获取有效的引用记录
        var records = await qs.Where(e => !string.IsNullOrEmpty(e.Referrer))
            .Select(e => e.Referrer)
            .ToListAsync();

        // 在内存中进行 Uri 解析和分组
        return records
            .Select(r => {
                try {
                    return new Uri(r!).Host;
                }
                catch {
                    return r;
                } // 处理无效的 URL
            })
            .GroupBy(host => host)
            .Select(g => new ReferrerDomain { Domain = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToList();
    }

    /// <summary>
    /// 环比增长分析
    /// <para>计算当前周期与上一周期的访问量对比</para>
    /// </summary>
    public async Task<GrowthRate> GetGrowthRate(VisitRecordParameters p, int days = 7) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-days);
        var prevStartDate = startDate.AddDays(-days);

        var currentPeriodCount = await qs
            .Where(e => e.Time.Date >= startDate && e.Time.Date <= endDate)
            .CountAsync();

        var prevPeriodCount = await qs
            .Where(e => e.Time.Date >= prevStartDate && e.Time.Date < startDate)
            .CountAsync();

        var growthRate = prevPeriodCount == 0 ? 0 : (currentPeriodCount - prevPeriodCount) * 100.0 / prevPeriodCount;

        return new GrowthRate {
            CurrentPeriodCount = currentPeriodCount,
            PrevPeriodCount = prevPeriodCount,
            GrowthPercentage = growthRate,
            Days = days
        };
    }

    /// <summary>
    /// 跳出率分析
    /// <para>计算只访问一个页面就离开的会话比例</para>
    /// </summary>
    public async Task<BounceRateStats> GetBounceRate(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);

        // 按IP和会话时间分组，计算每个会话的页面访问数
        var sessions = await qs
            .GroupBy(e => new { e.Ip, SessionTime = e.Time.Date.AddHours(e.Time.Hour) })
            .Select(g => new { g.Key.Ip, g.Key.SessionTime, PageViews = g.Count() })
            .ToListAsync();

        var totalSessions = sessions.Count;
        var bounceSessions = sessions.Count(s => s.PageViews == 1);
        var bounceRate = totalSessions == 0 ? 0 : (double)bounceSessions / totalSessions * 100;

        return new BounceRateStats {
            TotalSessions = totalSessions,
            BounceSessions = bounceSessions,
            BounceRate = bounceRate
        };
    }

    /// <summary>
    /// 首次访问分析
    /// <para>统计新访客与回访者的比例</para>
    /// </summary>
    public async Task<FirstVisitStats> GetFirstVisitStats(VisitRecordParameters p, int days) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);

        // 获取所有IP的首次访问时间
        var firstVisits = await qs
            .GroupBy(e => e.Ip)
            .Select(g => new { Ip = g.Key, FirstVisit = g.Min(e => e.Time) })
            .ToListAsync();

        // 当前时间范围内的访问记录
        var periodStart = DateTime.Today.AddDays(-days);
        var currentPeriodVisits = await qs
            .Where(e => e.Time >= periodStart)
            .Select(e => new { e.Ip, e.Time })
            .ToListAsync();

        // 计算新访客数量
        var newVisitors = currentPeriodVisits
            .GroupBy(e => e.Ip)
            .Count(g => firstVisits.First(f => f.Ip == g.Key).FirstVisit >= periodStart);

        // 计算回访者数量
        var returningVisitors = currentPeriodVisits
            .Select(e => e.Ip)
            .Distinct()
            .Count() - newVisitors;

        return new FirstVisitStats {
            NewVisitors = newVisitors,
            ReturningVisitors = returningVisitors,
            TotalVisitors = newVisitors + returningVisitors,
            NewVisitorsPercentage = (double)newVisitors / (newVisitors + returningVisitors) * 100
        };
    }

    /// <summary>
    /// 技术分布统计
    /// <para>统计浏览器、设备、操作系统的分布情况</para>
    /// </summary>
    public async Task<TechDistribution> GetTechDistribution(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);

        var browserStats = await qs
            .GroupBy(e => e.UserAgentInfo.UserAgent.Family)
            .Select(g => new { Browser = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();

        var osStats = await qs
            .GroupBy(e => e.UserAgentInfo.OS.Family)
            .Select(g => new { OS = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();

        var deviceStats = await qs
            .GroupBy(e => e.UserAgentInfo.Device.Family)
            .Select(g => new { Device = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();

        return new TechDistribution {
            Browsers = browserStats.Select(b => new NameCountPair { Name = b.Browser, Count = b.Count }).ToList(),
            OperatingSystems = osStats.Select(o => new NameCountPair { Name = o.OS, Count = o.Count }).ToList(),
            Devices = deviceStats.Select(d => new NameCountPair { Name = d.Device, Count = d.Count }).ToList()
        };
    }

    /// <summary>
    /// 慢请求排行
    /// <para>获取响应时间最长的请求列表</para>
    /// </summary>
    public async Task<List<SlowRequest>> GetSlowRequests(VisitRecordParameters p, int top = 10) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);

        return await qs
            .OrderByDescending(e => e.ResponseTimeMs)
            .Take(top)
            .Select(e => new SlowRequest {
                Path = e.RequestPath,
                Method = e.RequestMethod,
                ResponseTimeMs = e.ResponseTimeMs,
                Time = e.Time,
                StatusCode = e.StatusCode
            })
            .ToListAsync();
    }

    /// <summary>
    /// 转化行为分析
    /// <para>分析用户从入口页到目标页面的转化路径</para>
    /// </summary>
    public async Task<ConversionStats> GetConversionStats(VisitRecordParameters p, string targetPath) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);

        // 先获取基础数据到内存
        var records = await qs
            .Select(e => new { e.Ip, e.RequestPath, e.Time })
            .ToListAsync();

        // 在内存中进行复杂处理
        // 按IP和会话分组
        var sessions = records
            .GroupBy(e => e.Ip)
            .Select(g => new {
                Ip = g.Key,
                Paths = g.OrderBy(e => e.Time).Select(e => e.RequestPath).ToList()
            })
            .ToList();

        // 计算转化率
        var totalSessions = sessions.Count;
        var convertedSessions = sessions.Count(s => s.Paths.Contains(targetPath));
        var conversionRate = totalSessions == 0 ? 0 : (double)convertedSessions / totalSessions * 100;

        // 计算顶部入口页面
        var topEntryPages = sessions
            .Where(s => s.Paths.Count > 0)
            .GroupBy(s => s.Paths.First())
            .Select(g => new NameCountPair { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToList();

        return new ConversionStats {
            TargetPath = targetPath,
            TotalSessions = totalSessions,
            ConvertedSessions = convertedSessions,
            ConversionRate = conversionRate,
            TopEntryPages = topEntryPages
        };
    }
}
