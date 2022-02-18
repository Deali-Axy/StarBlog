using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

public class Category {
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    public string Name { get; set; }
    
    public int ParentId { get; set; }
    public Category? Parent { get; set; }

    public List<Post> Posts { get; set; }
}