using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models; 

public class TopPost {
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }
    public string PostId { get; set; }
    public Post Post { get; set; }
}