namespace StarBlog.Web.ViewModels.Blog;

public class PostCreationDto {
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
    /// 分类ID
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// ZIP编码
    /// </summary>
    public string ZipEncoding { get; set; } = "utf-8";
}