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

    public VisitRecord? GetById(int id) {
        var item = _repo.Where(a => a.Id == id).First();
        return item;
    }

    public List<VisitRecord> GetAll() {
        return _repo.Select.OrderByDescending(a => a.Time).ToList();
    }

    public IPagedList<VisitRecord> GetPagedList(VisitRecordQueryParameters param) {
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

        return querySet.ToList().ToPagedList(param.Page, param.PageSize);
    }

    /// <summary>
    /// 总览数据
    /// </summary>
    /// <returns></returns>
    public object Overview() {
        ISelect<VisitRecord> GetQuerySet() => _repo.Where(a => !a.RequestPath.StartsWith("/Api"));

        return new {
            TotalVisit = GetQuerySet().Count(),
            TodayVisit = GetQuerySet().Where(a => a.Time.Date == DateTime.Today).Count(),
            YesterdayVisit = GetQuerySet().Where(a => a.Time.Date == DateTime.Today.AddDays(-2).Date).Count(),
            Daily = GetQuerySet().GroupBy(a => a.Time.Date).ToList(a => new {
                time = $"{a.Key.Year}-{a.Key.Month}-{a.Key.Day}",
                count = a.Count()
            })
        };
    }

    /// <summary>
    /// 统计数据
    /// </summary>
    /// <returns></returns>
    public object Stats(DateTime date) {
        var data = _repo.Where(
            a => a.Time.Date == date.Date
                 && !a.RequestPath.StartsWith("/Api")
        );
        return new {
            Count = data.Count()
        };
    }
}