using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models; 

public class User {
    [Column(IsIdentity = false, IsPrimary = true)]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
}