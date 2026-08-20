namespace StarBlog.Application.ViewModels.Blog;

public class PostUpdateDto {
    public string Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 文章链接，设置后可以通过以下形式访问文章
    /// <para> http://starblog.com/p/post-slug1 </para>
    /// </summary>
    public string? Slug { get; set; }
    
    /// <summary>
    /// 文章标记
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// 是否发表（不发表的话就是草稿状态）
    /// </summary>
    public bool IsPublish { get; set; }

    /// <summary>
    /// 梗概
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// 内容（markdown格式）
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// 分类ID
    /// </summary>
    public int CategoryId { get; set; }
}