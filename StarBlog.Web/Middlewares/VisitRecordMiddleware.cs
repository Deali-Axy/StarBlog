using FreeSql;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;

namespace StarBlog.Web.Middlewares;

public class VisitRecordMiddleware {
    private readonly RequestDelegate _next;

    public VisitRecordMiddleware(RequestDelegate requestDelegate) {
        _next = requestDelegate;
    }

    public Task Invoke(HttpContext context, IBaseRepository<VisitRecord> visitRecordRepo) {
        var request = context.Request;
        var response = context.Response;

        visitRecordRepo.InsertAsync(new VisitRecord {
            Ip = context.GetRemoteIPAddress()?.ToString().Split(":")?.Last(),
            RequestPath = request.Path,
            RequestQueryString = request.QueryString.Value,
            RequestMethod = request.Method,
            UserAgent = request.Headers.UserAgent,
            Time = DateTime.Now
        });
        
        return _next(context);
    }
}