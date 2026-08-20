using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Modules.Analytics.Domain;
using StarBlog.Api.Tests.Infrastructure;

namespace StarBlog.Api.Tests.Modules;

/// <summary>
/// 访问统计查询返回契约而不是实体，并且能区分页面 PV 与接口访问。
/// </summary>
public sealed class AnalyticsFeatureTests : IClassFixture<StarBlogApiFactory> {
    private readonly StarBlogApiFactory _factory;
    private readonly HttpClient _client;

    public AnalyticsFeatureTests(StarBlogApiFactory factory) {
        _factory = factory;
        _client = factory.CreateClient();
        SeedVisitRecords();
    }

    [Fact]
    public async Task Visit_records_list_uses_contract_shape() {
        var response = await _client.GetAsync("/api/v1/admin/visit-records?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("totalCount").GetInt32() >= 2);
        var first = body.GetProperty("items").EnumerateArray().First();
        Assert.True(first.TryGetProperty("requestPath", out _));
        Assert.True(first.TryGetProperty("isSpider", out _));
        Assert.False(first.TryGetProperty("userAgentInfo", out _), "不应把 UA 值对象原样暴露给 API");
        Assert.False(first.TryGetProperty("ipInfo", out _), "不应把 IP 值对象原样暴露给 API");
    }

    [Fact]
    public async Task Overview_counts_human_page_views_separately_from_api() {
        var overview = await _client.GetFromJsonAsync<JsonElement>("/api/v1/admin/visit-records/reports/overview");
        Assert.True(overview.GetProperty("total").GetInt32() >= 3);
        Assert.True(overview.GetProperty("pv").GetInt32() >= 1);
        Assert.True(overview.GetProperty("api").GetInt32() >= 1);
        Assert.True(overview.GetProperty("spider").GetInt32() >= 1);
    }

    [Fact]
    public async Task Daily_trend_and_runtime_statistics_return_ok() {
        var trend = await _client.GetAsync("/api/v1/admin/visit-records/reports/daily-trends?days=7");
        Assert.Equal(HttpStatusCode.OK, trend.StatusCode);
        var runtime = await _client.GetFromJsonAsync<JsonElement>("/api/v1/admin/runtime/statistics");
        Assert.False(string.IsNullOrWhiteSpace(runtime.GetProperty("machineName").GetString()));
        Assert.True(runtime.GetProperty("workingSet").GetInt64() > 0);
    }

    /// <summary>写入页面访问、接口访问和爬虫各一条，覆盖统计分类。</summary>
    private void SeedVisitRecords() {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StarBlogDbContext>();
        if (db.VisitRecords.Any()) return;
        db.VisitRecords.AddRange(
            new VisitRecord {
                Ip = "1.1.1.1",
                RequestPath = "/blog",
                RequestMethod = "GET",
                Time = DateTime.Now,
                StatusCode = 200,
                UserAgentInfo = new UserAgentInfo { Device = new DeviceInfo { IsSpider = false } }
            },
            new VisitRecord {
                Ip = "1.1.1.1",
                RequestPath = "/api/v1/posts",
                RequestMethod = "GET",
                Time = DateTime.Now,
                StatusCode = 200,
                UserAgentInfo = new UserAgentInfo { Device = new DeviceInfo { IsSpider = false } }
            },
            new VisitRecord {
                Ip = "8.8.8.8",
                RequestPath = "/blog",
                RequestMethod = "GET",
                Time = DateTime.Now,
                StatusCode = 200,
                UserAgent = "Bytespider",
                UserAgentInfo = new UserAgentInfo { Device = new DeviceInfo { IsSpider = true } }
            });
        db.SaveChanges();
    }
}
