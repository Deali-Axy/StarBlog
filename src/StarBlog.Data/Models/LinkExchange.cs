using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// 友情链接申请记录
/// </summary>
public class LinkExchange {
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    /// <summary>
    /// 网站名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 介绍
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 网址
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 站长
    /// </summary>
    public string WebMaster { get; set; }

    /// <summary>
    /// 联系邮箱
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 是否已验证
    /// <para>友情链接需要验证后才显示在网站上</para>
    /// </summary>
    public bool Verified { get; set; } = false;

    /// <summary>
    /// 原因
    /// <para>如果验证不通过的话，可能会附上原因</para>
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 申请时间
    /// </summary>
    public DateTime ApplyTime { get; set; } = DateTime.Now;
}