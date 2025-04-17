using System.Linq.Dynamic.Core;
using IP2Region.Net.Abstractions;
using Microsoft.EntityFrameworkCore;
using StarBlog.Data;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels.QueryFilters;
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
        if (!string.IsNullOrWhiteSpace(item?.Ip)) {
            var result =_searcher.Search(item.Ip);
            Console.WriteLine($"ip2region result: {result}");
        }

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

        // to do 不能这样分页，得用数据库分页，不然性能很差 - 2023-12-1 09:53:50 搞定
        // return (await querySet.ToListAsync()).ToPagedList(param.Page, param.PageSize);

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