using IP2Region.Net.Abstractions;
using IP2Region.Net.XDB;

namespace StarBlog.Web.Extensions;

public static class ConfigureIp2Region {
    public static void AddIp2Region(this IServiceCollection services) {
        services.AddSingleton<ISearcher>(new Searcher(
            CachePolicy.VectorIndex,
            Path.Combine(Environment.CurrentDirectory, "ip2region.xdb")
        ));
    }
}