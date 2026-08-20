namespace StarBlog.Api.Modules.Analytics;

/// <summary>访问总览。</summary>
public sealed class VisitOverviewResponse {
    public int Total { get; init; }
    public int Pv { get; init; }
    public int Uv { get; init; }
    public int Api { get; init; }
    public int Spider { get; init; }
}

/// <summary>按天趋势。</summary>
public sealed class DailyTrendResponse {
    public DateTime Date { get; init; }
    public int Pv { get; init; }
    public int Uv { get; init; }
}

/// <summary>单条访问记录的对外契约，不暴露 EF 实体与值对象图。</summary>
public sealed class VisitRecordResponse {
    public long Id { get; init; }
    public string? Ip { get; init; }
    public string? Country { get; init; }
    public string? Province { get; init; }
    public string? City { get; init; }
    public string? Isp { get; init; }
    public required string RequestPath { get; init; }
    public string? RequestQueryString { get; init; }
    public required string RequestMethod { get; init; }
    public string? UserAgent { get; init; }
    public string? OsFamily { get; init; }
    public string? DeviceFamily { get; init; }
    public string? BrowserFamily { get; init; }
    public bool IsSpider { get; init; }
    public DateTime Time { get; init; }
    public int StatusCode { get; init; }
    public int ResponseTimeMs { get; init; }
    public string? Referrer { get; init; }
}

/// <summary>当前进程运行时快照。</summary>
public sealed class RuntimeStatisticsResponse {
    public required string MachineName { get; init; }
    public long WorkingSet { get; init; }
    public long GcMemory { get; init; }
    public int ThreadCount { get; init; }
    public DateTime StartedAt { get; init; }
}
