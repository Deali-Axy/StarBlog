using StarBlog.Web.Models;

namespace StarBlog.Web.Services; 

public class CrawlService {
    private readonly IHttpClientFactory _httpClientFactory;

    public CrawlService(IHttpClientFactory httpClientFactory) {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetPoem() {
        const string url = "http://www.sblt.deali.cn:15911/poem/simple";
        var http = _httpClientFactory.CreateClient();
        return await http.GetStringAsync(url);
    }

    public async Task<string> GetHitokoto() {
        const string url = "http://www.sblt.deali.cn:15911/hitokoto/get";
        var http = _httpClientFactory.CreateClient();
        var obj = await http.GetFromJsonAsync<DataAcqResp<List<Hitokoto>>>(url);
        return obj?.Data[0].Content ?? "(未能获取一言)";
    }
}