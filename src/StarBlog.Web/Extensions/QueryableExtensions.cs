using Microsoft.EntityFrameworkCore.DynamicLinq;
using StarBlog.Web.Criteria;
using X.PagedList;
using X.PagedList.EF;
using X.PagedList.Extensions;

namespace StarBlog.Web.Extensions;

public static class QueryableExtensions {
    public static Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, QueryParameters param) {
        return query.ToPagedListAsync(param.Page, param.PageSize);
    }
}