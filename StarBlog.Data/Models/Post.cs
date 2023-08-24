using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// 博客文章
/// </summary>
public class Post {
    [Column(IsIdentity = false, IsPrimary = true)]
    public string Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 文章链接，设置后可以通过以下形式访问文章
    /// <para> http://starblog.com/p/post-slug1 </para>
    /// </summary>
    [MaxLength(150)]
    public string? Slug { get; set; }
    
    /// <summary>
    /// 文章状态，提取原markdown文件的文件名前缀，用于区分文章状态，例子如下
    /// <para>《（未完成）StarBlog博客开发笔记(3)：模型设计》</para>
    /// <para>《（未发布）StarBlog博客开发笔记(3)：模型设计》</para>
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 是否发表（不发表的话就是草稿状态）
    /// </summary>
    public bool IsPublish { get; set; }

    /// <summary>
    /// 梗概
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 内容（markdown格式）
    /// </summary>
    [MaxLength(-1)]
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

    /// <summary>
    /// 文章的分类层级, 其内容类似这样 `1,2,3` , 用逗号分隔开分类ID
    /// </summary>
    public string? Categories { get; set; }
}