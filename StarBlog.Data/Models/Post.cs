using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// 博客文章
/// </summary>
public class Post {
    [Column(IsIdentity = false, IsPrimary = true)]
    public string Id { get; set; }

    public string Title { get; set; }
    
    public string Summary { get; set; }
    public string Content { get; set; }
    public string Path { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; }
}