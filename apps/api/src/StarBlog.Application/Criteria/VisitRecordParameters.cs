using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace StarBlog.Application.Criteria;

public class VisitRecordParameters : QueryParameters {
    /// <summary>
    /// 排序字段
    /// </summary>
    public new string? SortBy { get; set; } = "-Time";

    /// <summary>
    /// 排除 API 接口
    /// </summary>
    public bool? ExcludeApi { get; set; } = false;

    /// <summary>
    /// 排除内网IP
    /// </summary>
    public bool? ExcludeIntranetIp { get; set; }

    public DateTime? Date { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    /// <summary>
    /// 国家
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// 省份
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// 运营商
    /// </summary>
    public string? Isp { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    [JsonPropertyName("os")]
    [JsonProperty("os")]
    public string? OS { get; set; }

    /// <summary>
    /// 设备
    /// </summary>
    public string? Device { get; set; }

    /// <summary>
    /// UA
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 是否爬虫
    /// </summary>
    public bool? IsSpider { get; set; }
}