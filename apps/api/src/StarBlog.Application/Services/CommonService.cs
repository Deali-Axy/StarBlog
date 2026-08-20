using System.Net;
using StarBlog.Content.Utils;

namespace StarBlog.Application.Services;

/// <summary>
/// 一些公共服务
/// </summary>
public class CommonService {
    private readonly ILogger<CommonService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public CommonService(ILogger<CommonService> logger, IHttpClientFactory httpClientFactory) {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="url"></param>
    /// <param name="savePath">保存路径，需要完整路径</param>
    /// <returns></returns>
    public async Task<string?> DownloadFileAsync(string url, string savePath) {
        var httpClient = _httpClientFactory.CreateClient();
        try {
            var resp = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            var fileName = GuidUtils.GuidTo16String() + Path.GetExtension(url);
            var filePath = Path.Combine(savePath, WebUtility.UrlEncode(fileName));
            await using var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            await resp.Content.CopyToAsync(fs);

            return fileName;
        }
        catch (Exception ex) {
            _logger.LogError("下载文件出错，信息：{Error}", ex);
            return null;
        }
    }
}
