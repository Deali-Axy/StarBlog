using System.Collections.Concurrent;
using IP2Region.Net.Abstractions;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Modules.Analytics.Domain;
using UAParser;

namespace StarBlog.Api.Modules.Analytics;

/// <summary>
/// 将访问记录放入内存队列，由 Worker 批量写入数据库。
/// </summary>
public sealed class VisitRecordQueue {
    private readonly ConcurrentQueue<VisitRecord> _queue = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISearcher _searcher;
    private readonly ILogger<VisitRecordQueue> _logger;
    private readonly Parser _parser = Parser.GetDefault();

    public VisitRecordQueue(IServiceScopeFactory scopeFactory, ISearcher searcher, ILogger<VisitRecordQueue> logger) {
        _scopeFactory = scopeFactory;
        _searcher = searcher;
        _logger = logger;
    }

    public void Enqueue(VisitRecord record) => _queue.Enqueue(record);

    /// <summary>批量解析 IP/UA 并写入数据库。每次通过独立 scope 获取 DbContext。</summary>
    public async Task FlushAsync(CancellationToken cancellationToken) {
        if (_queue.IsEmpty) {
            await Task.Delay(1000, cancellationToken);
            return;
        }

        var batch = new List<VisitRecord>();
        while (batch.Count < 10 && _queue.TryDequeue(out var record)) {
            Enrich(record);
            batch.Add(record);
        }

        try {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StarBlogDbContext>();
            db.VisitRecords.AddRange(batch);
            await db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("访问日志写入 {Count} 条", batch.Count);
        }
        catch (Exception exception) {
            _logger.LogError(exception, "访问日志写入失败");
        }
    }

    private void Enrich(VisitRecord record) {
        if (!string.IsNullOrWhiteSpace(record.Ip)) {
            var result = _searcher.Search(record.Ip);
            if (!string.IsNullOrWhiteSpace(result)) {
                var parts = result.Split('|');
                if (parts.Length >= 5) {
                    record.IpInfo = new IpInfo {
                        Country = parts[0],
                        RegionCode = parts[1],
                        Province = parts[2],
                        City = parts[3],
                        Isp = parts[4]
                    };
                }
            }
        }

        if (string.IsNullOrWhiteSpace(record.UserAgent)) return;
        var client = _parser.Parse(record.UserAgent);
        record.UserAgentInfo = new UserAgentInfo {
            OS = new OsInfo {
                Family = client.OS.Family,
                Major = client.OS.Major,
                Minor = client.OS.Minor,
                Patch = client.OS.Patch,
                PatchMinor = client.OS.PatchMinor
            },
            Device = new DeviceInfo {
                Brand = client.Device.Brand,
                Family = client.Device.Family,
                Model = client.Device.Model,
                IsSpider = client.Device.IsSpider || (client.UA.Family?.Contains("bytespider", StringComparison.OrdinalIgnoreCase) ?? false)
            },
            UserAgent = new AgentInfo {
                Family = client.UA.Family,
                Major = client.UA.Major,
                Minor = client.UA.Minor,
                Patch = client.UA.Patch
            }
        };
    }
}
