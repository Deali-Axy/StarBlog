namespace StarBlog.Contrib.CLRStats;

/// <summary>
/// 获取一个统计 .NET 应用资源使用情况的插件，包含：CPU 使用率、GC、线程情况，支持通过 Web 请求获取状态信息（可以自定义访问路径和身份验证），数据将以 JSON 格式返回。
/// <para></para>
/// <para>原项目地址：https://github.com/itsvse/CLRStats</para>
/// </summary>
public static class CLRStatsUtils {
    private static readonly CLRStatsModel CLrStatsModel = new CLRStatsModel();

    public static CLRStatsModel GetCurrentClrStats() {
        return CLrStatsModel;
    }

    public static string GetCurrentClrStatsToJson() {
        return GetCurrentClrStats().ToJson();
    }
}