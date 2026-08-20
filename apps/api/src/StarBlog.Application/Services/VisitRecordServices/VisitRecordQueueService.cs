using System.Collections.Concurrent;
using AutoMapper;
using IP2Region.Net.Abstractions;
using StarBlog.Data;
using StarBlog.Data.Models;
using UAParser;

namespace StarBlog.Application.Services.VisitRecordServices;

public class VisitRecordQueueService {
    private readonly ConcurrentQueue<VisitRecord> _logQueue = new ConcurrentQueue<VisitRecord>();
    private readonly ILogger<VisitRecordQueueService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISearcher _searcher;
    private readonly IMapper _mapper;
    private readonly Parser _uaParser = Parser.GetDefault();

    /// <summary>
    /// 批量大小
    /// </summary>
    private const int BatchSize = 10;

    public VisitRecordQueueService(ILogger<VisitRecordQueueService> logger, IServiceScopeFactory scopeFactory,
        ISearcher searcher, IMapper mapper) {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _searcher = searcher;
        _mapper = mapper;
    }

    // 将日志加入队列
    public void EnqueueLog(VisitRecord log) {
        _logQueue.Enqueue(log);
    }

    // 定期批量写入数据库的方法
    public async Task WriteLogsToDatabaseAsync(CancellationToken cancellationToken) {
        if (_logQueue.IsEmpty) {
            // 暂时等待，避免高频次无意义的检查
            await Task.Delay(1000, cancellationToken);
            return;
        }

        var batch = new List<VisitRecord>();
        // 从队列中取出一批日志
        while (_logQueue.TryDequeue(out var log) && batch.Count < BatchSize) {
            log = InflateIpRegion(log);
            log = InflateUA(log);
            batch.Add(log);
        }

        try {
            using var scope = _scopeFactory.CreateScope();
            var dbCtx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await using var transaction = await dbCtx.Database.BeginTransactionAsync(cancellationToken);
            try {
                dbCtx.VisitRecords.AddRange(batch);
                await dbCtx.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("访问日志 Successfully wrote {BatchCount} logs to the database", batch.Count);
            }
            catch (Exception) {
                await transaction.RollbackAsync(cancellationToken);
                // 重新抛出异常，确保外部捕获
                throw;
            }
        }
        catch (Exception ex) {
            _logger.LogError(ex, "访问日志 Error writing logs to the database: {ExMessage}", ex.Message);
        }
    }

    private VisitRecord InflateIpRegion(VisitRecord log) {
        if (string.IsNullOrWhiteSpace(log.Ip)) return log;

        var result = _searcher.Search(log.Ip);
        if (string.IsNullOrWhiteSpace(result)) return log;

        var parts = result.Split('|');
        log.IpInfo = new IpInfo {
            Country = parts[0],
            RegionCode = parts[1],
            Province = parts[2],
            City = parts[3],
            Isp = parts[4]
        };

        return log;
    }

    /// <summary>
    /// 解析用户代理信息
    /// </summary>
    /// <param name="log">访问记录</param>
    /// <returns>填充了用户代理信息的访问记录</returns>
    private VisitRecord InflateUA(VisitRecord log) {
        // 检查UserAgent是否为null或空字符串，避免UAParser抛出ArgumentNullException
        if (string.IsNullOrWhiteSpace(log.UserAgent)) {
            // 如果UserAgent为空，设置UserAgentInfo为默认值
            log.UserAgentInfo = new UserAgentInfo();
            return log;
        }
        
        var c = _uaParser.Parse(log.UserAgent);
        log.UserAgentInfo = _mapper.Map<UserAgentInfo>(c);
        if (!string.IsNullOrWhiteSpace(log.UserAgentInfo.UserAgent.Family)
            && log.UserAgentInfo.UserAgent.Family.ToLower().Contains("bytespider")) {
            log.UserAgentInfo.Device.IsSpider = true;
        }
        return log;
    }
}