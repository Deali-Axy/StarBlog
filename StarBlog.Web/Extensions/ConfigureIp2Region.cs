using IP2Region.Net.Abstractions;
using IP2Region.Net.XDB;

namespace StarBlog.Web.Extensions;

public static class ConfigureIp2Region {
    public static void AddIp2Region(this IServiceCollection services) {
        services.AddSingleton<ISearcher>(new Searcher(
            // 这里选择整个数据文件都缓存到内存里，性能更高，也能实现线程安全
            // 事实上 C# 版实现不缓存也是线程安全的（作者说的）
            CachePolicy.Content,
            Path.Combine(Environment.CurrentDirectory, "ip2region.xdb")
        ));
    }
}