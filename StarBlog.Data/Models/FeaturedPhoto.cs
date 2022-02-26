using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

public class FeaturedPhoto {
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    public string PhotoId { get; set; }
    public Photo Photo { get; set; }
}