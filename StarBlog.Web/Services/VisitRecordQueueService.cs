using System.Collections.Concurrent;
using StarBlog.Data;
using StarBlog.Data.Models;

namespace StarBlog.Web.Services;

public class VisitRecordQueueService {
    private readonly ConcurrentQueue<VisitRecord> _logQueue = new ConcurrentQueue<VisitRecord>();
    private readonly ILogger<VisitRecordQueueService> _logger;
    private readonly AppDbContext _context;

    /// <summary>
    /// 批量大小可调整
    /// </summary>
    private const int BatchSize = 100;

    public VisitRecordQueueService(ILogger<VisitRecordQueueService> logger, AppDbContext context) {
        _logger = logger;
        _context = context;
    }

    // 将日志加入队列
    public void EnqueueLog(VisitRecord log) {
        _logQueue.Enqueue(log);
    }

    // 定期批量写入数据库的方法
    public async Task WriteLogsToDatabaseAsync(CancellationToken cancellationToken) {
        while (!cancellationToken.IsCancellationRequested) {
            if (!_logQueue.IsEmpty) {
                var batch = new List<VisitRecord>();
                // 从队列中取出一批日志
                while (_logQueue.TryDequeue(out var log) && batch.Count < BatchSize) {
                    batch.Add(log);
                }

                try {
                    _context.VisitRecords.AddRange(batch);
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation($"Successfully wrote {batch.Count} logs to the database.");
                }
                catch (Exception ex) {
                    _logger.LogError($"Error writing logs to the database: {ex.Message}");
                }
            }

            // 暂停一会再继续检查
            await Task.Delay(5000, cancellationToken); // 每5秒写一次日志
        }
    }
}