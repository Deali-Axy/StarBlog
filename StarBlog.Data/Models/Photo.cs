using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models; 

public class Photo {
    [Column(IsIdentity = false, IsPrimary = true)]
    public string Id { get; set; }
    public string Title { get; set; }
    public string Location { get; set; }
    public DateTime CreateTime { get; set; }
}