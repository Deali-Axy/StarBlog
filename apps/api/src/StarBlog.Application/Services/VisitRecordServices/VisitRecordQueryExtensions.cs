using StarBlog.Application.Criteria;

namespace StarBlog.Application.Services.VisitRecordServices;

public static class VisitRecordQueryExtensions {
    public static IQueryable<Data.Models.VisitRecord> ApplyFilters(
        this IQueryable<Data.Models.VisitRecord> query, VisitRecordParameters p
    ) {
        if (p.ExcludeApi == true) query = query.Where(r => !r.RequestPath.StartsWith("/api/"));
        if (p.ExcludeIntranetIp == true)
            // todo 这种判空逻辑可能会有问题，非内网IP可能 ISP 信息是null
            query = query.Where(r => r.IpInfo.Isp != null && !r.IpInfo.Isp.Contains("内网"));


        if (p.Date.HasValue) {
            var startDate = p.Date.Value.Date;
            var endDate = startDate.AddDays(1);
            query = query.Where(r => r.Time >= startDate && r.Time < endDate);
        }

        if (p.From.HasValue) query = query.Where(r => r.Time >= p.From.Value);
        if (p.To.HasValue) query = query.Where(r => r.Time <= p.To.Value);

        // IP信息相关过滤
        if (!string.IsNullOrEmpty(p.Country)) query = query.Where(r => r.IpInfo.Country == p.Country);
        if (!string.IsNullOrEmpty(p.Province)) query = query.Where(r => r.IpInfo.Province == p.Province);
        if (!string.IsNullOrEmpty(p.City)) query = query.Where(r => r.IpInfo.City == p.City);
        if (!string.IsNullOrEmpty(p.Isp)) query = query.Where(r => r.IpInfo.Isp == p.Isp);

        // 用户代理相关过滤
        if (!string.IsNullOrEmpty(p.OS)) query = query.Where(r => r.UserAgentInfo.OS.Family == p.OS);
        if (!string.IsNullOrEmpty(p.Device)) query = query.Where(r => r.UserAgentInfo.Device.Family == p.Device);
        if (!string.IsNullOrEmpty(p.UserAgent))
            query = query.Where(r => r.UserAgent != null && r.UserAgent.Contains(p.UserAgent));
        if (p.IsSpider.HasValue) query = query.Where(r => r.UserAgentInfo.Device.IsSpider == p.IsSpider.Value);

        // 通用查询参数处理
        // 关键词搜索
        if (!string.IsNullOrEmpty(p.Search)) {
            query = query.Where(r =>
                r.Referrer != null && r.UserAgent != null && r.Ip != null && (
                    r.RequestPath.Contains(p.Search) ||
                    r.Referrer.Contains(p.Search) ||
                    r.UserAgent.Contains(p.Search) ||
                    r.Ip.Contains(p.Search))
            );
        }

        // 排序处理
        if (!string.IsNullOrEmpty(p.SortBy)) {
            // 处理排序方向，默认为升序，如果以'-'开头则为降序
            bool isDescending = p.SortBy.StartsWith("-");
            string sortField = isDescending ? p.SortBy[1..] : p.SortBy;

            // 根据排序字段进行排序
            switch (sortField.ToLower()) {
                case "time":
                    query = isDescending ? query.OrderByDescending(r => r.Time) : query.OrderBy(r => r.Time);
                    break;
                case "ip":
                    query = isDescending ? query.OrderByDescending(r => r.Ip) : query.OrderBy(r => r.Ip);
                    break;
                case "requestpath":
                    query = isDescending
                        ? query.OrderByDescending(r => r.RequestPath)
                        : query.OrderBy(r => r.RequestPath);
                    break;
                case "referer":
                    query = isDescending ? query.OrderByDescending(r => r.Referrer) : query.OrderBy(r => r.Referrer);
                    break;
                case "useragent":
                    query = isDescending ? query.OrderByDescending(r => r.UserAgent) : query.OrderBy(r => r.UserAgent);
                    break;
                default:
                    // 默认按时间降序排列
                    query = query.OrderByDescending(r => r.Time);
                    break;
            }
        }
        else {
            // 默认按时间降序排列
            query = query.OrderByDescending(r => r.Time);
        }

        return query;
    }
}