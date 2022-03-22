using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// 博客文章
/// </summary>
/// todo 增加文章状态字段
public class Post {
    [Column(IsIdentity = false, IsPrimary = true)]
    public string? Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 梗概
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 内容（markdown格式）
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 博客在导入前的相对路径
    /// <para>如："系列/AspNetCore开发笔记"</para>
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// 上次更新时间
    /// </summary>
    public DateTime LastUpdateTime { get; set; }

    /// <summary>
    /// 分类ID
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    public Category? Category { get; set; }

    public string? Categories { get; set; }
}