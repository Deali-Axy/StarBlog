namespace StarBlog.Api.Modules.Analytics.Domain;

/// <summary>
/// 访问记录。与业务数据共用同一个数据库。
/// </summary>
public sealed class VisitRecord {
    public long Id { get; set; }
    public string? Ip { get; set; }
    public IpInfo IpInfo { get; set; } = new();
    public string RequestPath { get; set; } = string.Empty;
    public string? RequestQueryString { get; set; }
    public string RequestMethod { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public UserAgentInfo UserAgentInfo { get; set; } = new();
    public DateTime Time { get; set; }
    public int StatusCode { get; set; }
    public int ResponseTimeMs { get; set; }
    public string? Referrer { get; set; }
}

public sealed class IpInfo {
    public string? RegionCode { get; set; }
    public string? Country { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? Isp { get; set; }
}

public sealed class UserAgentInfo {
    public OsInfo OS { get; set; } = new();
    public DeviceInfo Device { get; set; } = new();
    public AgentInfo UserAgent { get; set; } = new();
}

public sealed class OsInfo {
    public string? Family { get; set; }
    public string? Major { get; set; }
    public string? Minor { get; set; }
    public string? Patch { get; set; }
    public string? PatchMinor { get; set; }
}

public sealed class DeviceInfo {
    public string? Brand { get; set; }
    public string? Family { get; set; }
    public string? Model { get; set; }
    public bool IsSpider { get; set; }
}

public sealed class AgentInfo {
    public string? Family { get; set; }
    public string? Major { get; set; }
    public string? Minor { get; set; }
    public string? Patch { get; set; }
}
