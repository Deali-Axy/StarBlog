using System.Collections.Concurrent;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace DataProc.Services;

/// <summary>
/// 带速率限制的LLM服务包装器
/// </summary>
public class RateLimitedLLM {
    private readonly LLM _llm;
    private readonly ILogger<RateLimitedLLM> _logger;
    private readonly SemaphoreSlim _rateLimiter;
    private readonly ConcurrentQueue<DateTime> _requestTimes;
    private readonly int _maxRequestsPerMinute;
    private readonly TimeSpan _timeWindow;

    public RateLimitedLLM(LLM llm, ILogger<RateLimitedLLM> logger, int maxRequestsPerMinute = 10) {
        _llm = llm;
        _logger = logger;
        _maxRequestsPerMinute = maxRequestsPerMinute;
        _timeWindow = TimeSpan.FromMinutes(1);
        _rateLimiter = new SemaphoreSlim(1, 1);
        _requestTimes = new ConcurrentQueue<DateTime>();
    }

    /// <summary>
    /// 带速率限制的流式文本生成
    /// </summary>
    public async Task<IAsyncEnumerable<ChatResponseUpdate>> GenerateTextStreamAsync(string prompt) {
        await WaitForRateLimit();
        
        try {
            RecordRequest();
            return _llm.GenerateTextStreamAsync(prompt);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "LLM请求失败");
            throw;
        }
    }

    /// <summary>
    /// 等待速率限制
    /// </summary>
    private async Task WaitForRateLimit() {
        await _rateLimiter.WaitAsync();
        try {
            // 清理过期的请求记录
            CleanupOldRequests();
            
            // 检查是否超过速率限制
            if (_requestTimes.Count >= _maxRequestsPerMinute) {
                var oldestRequest = _requestTimes.TryPeek(out var oldest) ? oldest : DateTime.MinValue;
                var waitTime = _timeWindow - (DateTime.UtcNow - oldestRequest);
                
                if (waitTime > TimeSpan.Zero) {
                    _logger.LogWarning("达到速率限制，等待 {WaitTime} 秒", waitTime.TotalSeconds);
                    await Task.Delay(waitTime);
                    CleanupOldRequests();
                }
            }
        }
        finally {
            _rateLimiter.Release();
        }
    }

    /// <summary>
    /// 记录请求时间
    /// </summary>
    private void RecordRequest() {
        _requestTimes.Enqueue(DateTime.UtcNow);
    }

    /// <summary>
    /// 清理过期的请求记录
    /// </summary>
    private void CleanupOldRequests() {
        var cutoff = DateTime.UtcNow - _timeWindow;
        while (_requestTimes.TryPeek(out var oldest) && oldest < cutoff) {
            _requestTimes.TryDequeue(out _);
        }
    }
}
