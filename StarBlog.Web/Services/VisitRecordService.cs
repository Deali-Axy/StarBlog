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

    public async Task<VisitRecord?> GetById(int id) {
        var item = await _dbContext.VisitRecords.FirstOrDefaultAsync(e => e.Id == id);
        return item;
    }

    public async Task<List<VisitRecord>> GetAll() {
        return await _dbContext.VisitRecords.OrderByDescending(e => e.Time).ToListAsync();
    }

    public async Task<IPagedList<VisitRecord>> GetPagedList(VisitRecordQueryParameters param) {
        var querySet = _dbContext.VisitRecords.AsQueryable();

        // 搜索
        if (!string.IsNullOrEmpty(param.Search)) {
            querySet = querySet.Where(a => a.RequestPath.Contains(param.Search));
        }

        // 排序
        if (!string.IsNullOrEmpty(param.SortBy)) {
            var isDesc = param.SortBy.StartsWith("-");
            var orderByProperty = param.SortBy.Trim('-');
            if (isDesc) {
                orderByProperty = $"{orderByProperty} desc";
            }

            querySet = querySet.OrderBy(orderByProperty);
        }

        IPagedList<VisitRecord> pagedList = new StaticPagedList<VisitRecord>(
            await querySet.Page(param.Page, param.PageSize).ToListAsync(),
            param.Page, param.PageSize,
            Convert.ToInt32(await querySet.CountAsync())
        );
        return pagedList;
    }

    /// <summary>
    /// 总览数据
    /// </summary>
    /// <returns></returns>
    public async Task<object> Overview() {
        var querySet = _dbContext.VisitRecords
            .Where(e => !e.RequestPath.StartsWith("/Api"));

        return new {
            TotalVisit = await querySet.CountAsync(),
            TodayVisit = await querySet.Where(e => e.Time.Date == DateTime.Today).CountAsync(),
            YesterdayVisit = await querySet
                .Where(e => e.Time.Date == DateTime.Today.AddDays(-1).Date)
                .CountAsync()
        };
    }

    /// <summary>
    /// PV / UV 统计
    /// <para>PV（Page Views）：总访问数</para>
    /// <para>UV（Unique Visitors）：去重的 IP 数或去重的用户数</para>
    /// </summary>
    public async Task<PvUv> GetPvUv(DateTime date) {
        var q = _dbContext.VisitRecords.Where(e => e.Time.Date == date.Date);
        var pv = await q.CountAsync();
        var uv = await q.Select(e => e.Ip).Distinct().CountAsync();
        return new PvUv { Date = date.Date, PV = pv, UV = uv };
    }

    /// <summary>
    /// Top N 访问路径
    /// <para>找出某天或某段时间最热的页面／接口</para>
    /// </summary>
    public async Task<List<TopPath>> GetTopPaths(DateTime from, DateTime to, int top = 10) {
        return await _dbContext.VisitRecords
            .Where(e => e.Time >= from && e.Time < to)
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
    public async Task<object> GetResponseTimeStats(DateTime date) {
        var q = _dbContext.VisitRecords.Where(e => e.Time.Date == date.Date);
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
    public async Task<List<StatusCodeDistribution>> GetStatusCodeDistribution(DateTime date) {
        return await _dbContext.VisitRecords
            .Where(e => e.Time.Date == date.Date)
            .GroupBy(e => e.StatusCode)
            .Select(g => new StatusCodeDistribution { Code = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
    }

    /// <summary>
    /// 小时／分钟级趋势
    /// <para>按小时查看访问量变化</para>
    /// </summary>
    public async Task<List<HourlyTrend>> GetHourlyTrend(DateTime date) {
        return await _dbContext.VisitRecords
            .Where(e => e.Time.Date == date.Date)
            .GroupBy(e => e.Time.Hour)
            .Select(g => new HourlyTrend { Hour = g.Key, Count = g.Count() })
            .OrderBy(x => x.Hour)
            .ToListAsync();
    }


    /// <summary>
    /// 趋势数据
    /// </summary>
    /// <param name="days">查看最近几天的数据，默认7天</param>
    /// <returns></returns>
    public async Task<object> Trend(int days = 7) {
        var startDate = DateTime.Today.AddDays(-days).Date;
        return await _dbContext.VisitRecords
            .Where(e => !e.RequestPath.StartsWith("/Api"))
            .Where(e => e.Time.Date >= startDate)
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
    /// <returns></returns>
    public async Task<object> Stats(DateTime date) {
        return new {
            Count = await _dbContext.VisitRecords
                .Where(e => e.Time.Date == date)
                .Where(e => !e.RequestPath.StartsWith("/Api"))
                .CountAsync()
        };
    }
}