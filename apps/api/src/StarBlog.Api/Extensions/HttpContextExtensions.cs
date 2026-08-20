using System.Net;

namespace StarBlog.Api.Extensions;

public static class HttpContextExtensions {
    /// <summary>
    /// 获取IP地址
    /// <para>https://gist.github.com/jjxtra/3b240b31a1ed3ad783a7dcdb6df12c36</para>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="allowForwarded"></param>
    /// <returns></returns>
    public static IPAddress? GetRemoteIpAddress(this HttpContext context, bool allowForwarded = true) {
        if (allowForwarded) {
            var header = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault()
                         ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (IPAddress.TryParse(header, out var ip)) {
                return ip;
            }
        }

        return context.Connection.RemoteIpAddress;
    }
}