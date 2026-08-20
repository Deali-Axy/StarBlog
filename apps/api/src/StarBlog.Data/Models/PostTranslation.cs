using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// 博客文章翻译
/// </summary>
public class PostTranslation {
    [Column(IsIdentity = false, IsPrimary = true)]
    public string Id { get; set; }

    /// <summary>
    /// 关联文章ID
    /// </summary>
    public string PostId { get; set; }
    public Post Post { get; set; }

    /// <summary>
    /// 语言代码，如 "en", "ja"
    /// </summary>
    public string Language { get; set; }

    /// <summary>
    /// 翻译后的标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 翻译后的摘要
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 翻译后的 Markdown 内容
    /// </summary>
    [MaxLength(-1)]
    public string? Content { get; set; }

    public DateTime CreationTime { get; set; }
    public DateTime LastUpdateTime { get; set; }
}
