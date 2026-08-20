using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using StarBlog.Application.Criteria;
using X.PagedList;

namespace StarBlog.Application.Extensions;

public static class QueryableExtensions {
    public static async Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, QueryParameters param) {
        if (!param.EnablePaging) {
            var all = await query.ToListAsync();
            return new StaticPagedList<T>(all, 1, all.Count, all.Count);
        }

        var page = Math.Max(1, param.Page);
        var pageSize = Math.Clamp(param.PageSize, 1, QueryParameters.MaxPageSize);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new StaticPagedList<T>(items, page, pageSize, total);
    }
}
