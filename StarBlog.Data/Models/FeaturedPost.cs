using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// 推荐文章
/// </summary>
[Table(Name = "featured_post", OldName = "FeaturedPost")]
public class FeaturedPost {
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    public string PostId { get; set; }
    public Post Post { get; set; }
}