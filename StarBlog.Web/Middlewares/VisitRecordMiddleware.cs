using FreeSql;
using StarBlog.Data;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;

namespace StarBlog.Web.Middlewares;

public class VisitRecordMiddleware {
    private readonly RequestDelegate _next;
    private readonly VisitRecordQueueService _logQueue;

    public VisitRecordMiddleware(RequestDelegate requestDelegate, VisitRecordQueueService logQueue) {
        _next = requestDelegate;
        _logQueue = logQueue;
    }

    public Task Invoke(HttpContext context) {
        var request = context.Request;

        var item = new VisitRecord {
            Ip = context.GetRemoteIPAddress()?.ToString().Split(":")?.Last(),
            RequestPath = request.Path,
            RequestQueryString = request.QueryString.Value,
            RequestMethod = request.Method,
            UserAgent = request.Headers.UserAgent,
            Time = DateTime.Now
        };
        _logQueue.EnqueueLog(item);

        return _next(context);
    }
}