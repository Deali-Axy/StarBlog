using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace StarBlog.Web.ViewModels.QueryFilters;

public class VisitRecordQueryParameters : QueryParameters {
    /// <summary>
    /// 排序字段
    /// </summary>
    public new string? SortBy { get; set; } = "-Time";
    
    /// <summary>
    /// 排除 API 接口
    /// </summary>
    public bool ExcludeApi { get; set; } = false;
    public string? Country { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? Isp { get; set; }

    [JsonPropertyName("os")]
    [JsonProperty("os")]
    public string? OS { get; set; }
    public string? Device { get; set; }
    public string? UserAgent { get; set; }
    public bool? IsSpider { get; set; }
}