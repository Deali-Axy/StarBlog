using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// 访问记录
/// </summary>
[Table(Name = "visit_record")]
public class VisitRecord {
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

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
    public string UserAgent { get; set; }

    /// <summary>
    /// 时间
    /// </summary>
    public DateTime Time { get; set; }
}