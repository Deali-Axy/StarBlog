using System.Net;
using IP2Region.Net.Abstractions;

namespace StarBlog.Api.Modules.Analytics;

/// <summary>
/// 在没有 ip2region 数据文件时返回占位解析结果。
/// </summary>
public sealed class FakeIpSearcher : ISearcher {
    public const string FakeResult = "0|0|0|0|0";

    public string? Search(string ipStr) => FakeResult;
    public string? Search(IPAddress ipAddress) => FakeResult;
    public string? Search(uint ipAddress) => FakeResult;
    public int IoCount => 0;
    public void Dispose() {
    }
}
