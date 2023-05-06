using FreeSql;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels.QueryFilters;
using X.PagedList;

namespace StarBlog.Web.Services;

public class VisitRecordService {
    private readonly IBaseRepository<VisitRecord> _repo;

    public VisitRecordService(IBaseRepository<VisitRecord> repo) {
        _repo = repo;
    }

    public async Task<VisitRecord?> GetById(int id) {
        var item = await _repo.Where(a => a.Id == id).FirstAsync();
        return item;
    }

    public async Task<List<VisitRecord>> GetAll() {
        return await _repo.Select.OrderByDescending(a => a.Time).ToListAsync();
    }

    public async Task<IPagedList<VisitRecord>> GetPagedList(VisitRecordQueryParameters param) {
        var querySet = _repo.Select;

        // 搜索
        if (!string.IsNullOrEmpty(param.Search)) {
            querySet = querySet.Where(a => a.RequestPath.Contains(param.Search));
        }

        // 排序
        if (!string.IsNullOrEmpty(param.SortBy)) {
            // 是否升序
            var isAscending = !param.SortBy.StartsWith("-");
            var orderByProperty = param.SortBy.Trim('-');
            querySet = querySet.OrderByPropertyName(orderByProperty, isAscending);
        }

        // todo 不能这样分页，得用数据库分页，不然性能很差
        return (await querySet.ToListAsync()).ToPagedList(param.Page, param.PageSize);
    }

    /// <summary>
    /// 总览数据
    /// </summary>
    /// <returns></returns>
    public async Task<object> Overview() {
        return await _repo.Where(a => !a.RequestPath.StartsWith("/Api"))
            .ToAggregateAsync(g => new {
                TotalVisit = g.Count(),
                TodayVisit = g.Sum(g.Key.Time.Date == DateTime.Today ? 1 : 0),
                YesterdayVisit = g.Sum(g.Key.Time.Date == DateTime.Today.AddDays(-1).Date ? 1 : 0)
            });
    }

    /// <summary>
    /// 趋势数据
    /// </summary>
    /// <param name="days">查看最近几天的数据，默认7天</param>
    /// <returns></returns>
    public async Task<object> Trend(int days = 7) {
        return await _repo.Where(a => !a.RequestPath.StartsWith("/Api"))
            .Where(a => a.Time.Date > DateTime.Today.AddDays(-days).Date)
            .GroupBy(a => a.Time.Date)
            .ToListAsync(a => new {
                time = a.Key,
                date = $"{a.Key.Month}-{a.Key.Day}",
                count = a.Count()
            });
    }

    /// <summary>
    /// 统计数据
    /// </summary>
    /// <returns></returns>
    public async Task<object> Stats(DateTime date) {
        return await _repo.Where(
            a => a.Time.Date == date.Date
                 && !a.RequestPath.StartsWith("/Api")
        ).ToAggregateAsync(g => new {
            Count = g.Count()
        });
    }
}