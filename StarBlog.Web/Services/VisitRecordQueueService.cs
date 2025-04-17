﻿using System.Collections.Concurrent;
using IP2Region.Net.Abstractions;
using StarBlog.Data;
using StarBlog.Data.Models;

namespace StarBlog.Web.Services;

public class VisitRecordQueueService {
    private readonly ConcurrentQueue<VisitRecord> _logQueue = new ConcurrentQueue<VisitRecord>();
    private readonly ILogger<VisitRecordQueueService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISearcher _searcher;

    /// <summary>
    /// 批量大小
    /// </summary>
    private const int BatchSize = 10;

    public VisitRecordQueueService(ILogger<VisitRecordQueueService> logger, IServiceScopeFactory scopeFactory,
        ISearcher searcher) {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _searcher = searcher;
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
        log.Country = parts[0];
        log.RegionCode = parts[1];
        log.Province = parts[2];
        log.City = parts[3];
        log.Isp = parts[4];

        return log;
    }
}