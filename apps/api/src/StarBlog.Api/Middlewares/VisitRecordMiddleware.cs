using System.Diagnostics;
using StarBlog.Data.Models;
using StarBlog.Api.Extensions;
using StarBlog.Application.Services.VisitRecordServices;

namespace StarBlog.Api.Middlewares;

public class VisitRecordMiddleware {
    private readonly RequestDelegate _next;

    public VisitRecordMiddleware(RequestDelegate requestDelegate) {
        _next = requestDelegate;
    }

    public async Task Invoke(HttpContext context, VisitRecordQueueService logQueue) {
        var request = context.Request;

        // 记录开始时间
        var stopwatch = Stopwatch.StartNew();

        // 先构造一个空对象，后面填充
        var record = new VisitRecord {
            Ip = context.GetRemoteIpAddress()?.ToString(),
            RequestPath = request.Path,
            RequestQueryString = request.QueryString.Value,
            RequestMethod = request.Method,
            UserAgent = request.Headers.UserAgent,
            Time = DateTime.Now,
            Referrer = request.Headers.Referer, // 来源页面
        };

        // 在响应真正开始写到客户端前，补齐状态码和耗时
        context.Response.OnStarting(state => {
            var (rec, sw, queue) = (Tuple<VisitRecord, Stopwatch, VisitRecordQueueService>)state;
            // 结束计时
            sw.Stop();

            rec.StatusCode = context.Response.StatusCode;
            rec.ResponseTimeMs = (int)sw.ElapsedMilliseconds;

            // 入队
            queue.EnqueueLog(rec);
            return Task.CompletedTask;
        }, Tuple.Create(record, stopwatch, logQueue));

        // 执行后续管道
        await _next(context);
    }
}