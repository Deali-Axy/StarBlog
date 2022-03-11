namespace StarBlog.Web.ViewModels.Blog;

public class PostUpdateDto {
    public string Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }

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