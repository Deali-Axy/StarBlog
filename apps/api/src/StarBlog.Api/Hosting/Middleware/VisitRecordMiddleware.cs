using System.Diagnostics;
using StarBlog.Api.Infrastructure.Time;
using StarBlog.Api.Modules.Analytics;
using StarBlog.Api.Modules.Analytics.Domain;

namespace StarBlog.Api.Hosting.Middleware;

/// <summary>
/// 在响应开始写入前记录状态码和耗时，并入队访问日志。
/// </summary>
public sealed class VisitRecordMiddleware {
    private readonly RequestDelegate _next;

    public VisitRecordMiddleware(RequestDelegate next) {
        _next = next;
    }

    public async Task Invoke(HttpContext context, VisitRecordQueue queue, IClock clock) {
        var stopwatch = Stopwatch.StartNew();
        var record = new VisitRecord {
            Ip = context.Connection.RemoteIpAddress?.ToString(),
            RequestPath = context.Request.Path,
            RequestQueryString = context.Request.QueryString.Value,
            RequestMethod = context.Request.Method,
            UserAgent = context.Request.Headers.UserAgent,
            Time = clock.Now,
            Referrer = context.Request.Headers.Referer
        };

        context.Response.OnStarting(() => {
            stopwatch.Stop();
            record.StatusCode = context.Response.StatusCode;
            record.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            queue.Enqueue(record);
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
