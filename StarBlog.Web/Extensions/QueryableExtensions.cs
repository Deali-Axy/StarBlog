using Microsoft.EntityFrameworkCore.DynamicLinq;
using StarBlog.Web.Criteria;
using X.PagedList;

namespace StarBlog.Web.Extensions;

public static class QueryableExtensions {
    public static Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, QueryParameters param) {
        return query.ToPagedListAsync(param.Page, param.PageSize);
    }
}