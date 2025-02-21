using FreeSql;
using StarBlog.Data;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;

namespace StarBlog.Web.Middlewares;

public class VisitRecordMiddleware {
    private readonly RequestDelegate _next;

    public VisitRecordMiddleware(RequestDelegate requestDelegate) {
        _next = requestDelegate;
    }

    public Task Invoke(HttpContext context, VisitRecordQueueService logQueue) {
        var request = context.Request;

        var item = new VisitRecord {
            Ip = context.GetRemoteIPAddress()?.ToString().Split(":")?.Last(),
            RequestPath = request.Path,
            RequestQueryString = request.QueryString.Value,
            RequestMethod = request.Method,
            UserAgent = request.Headers.UserAgent,
            Time = DateTime.Now
        };
        logQueue.EnqueueLog(item);

        return _next(context);
    }
}