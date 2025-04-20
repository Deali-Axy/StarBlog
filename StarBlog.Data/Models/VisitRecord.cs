using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// 访问记录
/// </summary>
[Table(Name = "visit_record")]
public class VisitRecord {
    [Column(IsIdentity = true, IsPrimary = true)]
    public long Id { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    public string? Ip { get; set; }

    /// <summary>
    /// 请求路径
    /// </summary>
    public string RequestPath { get; set; }

    /// <summary>
    /// 请求参数
    /// </summary>
    public string? RequestQueryString { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    public string RequestMethod { get; set; }

    /// <summary>
    /// 用户设备
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// UA信息
    /// </summary>
    public UserAgentInfo UserAgentInfo { get; set; }

    /// <summary>
    /// 时间
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>HTTP 状态码</summary>
    public int StatusCode { get; set; }

    /// <summary>响应耗时（ms）</summary>
    public int ResponseTimeMs { get; set; }

    /// <summary>来源页面</summary>
    public string? Referrer { get; set; }

    // 区域
    public string? RegionCode { get; set; }
    public string? Country { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? Isp { get; set; }
}

public class UserAgentInfo {
    public OS OS { get; set; }
    public Device Device { get; set; }
    public UserAgent UserAgent { get; set; }
}

public class OS {
    public string? Family { get; set; }
    public string? Major { get; set; }
    public string? Minor { get; set; }
    public string? Patch { get; set; }
    public string? PatchMinor { get; set; }
}

public class Device {
    public string? Brand { get; set; }
    public string? Family { get; set; }
    public string? Model { get; set; }
    public bool IsSpider { get; set; }
}

public class UserAgent {
    public string? Family { get; set; }
    public string? Major { get; set; }
    public string? Minor { get; set; }
    public string? Patch { get; set; }
}