namespace StarBlog.Data.Models;

public abstract class ModelBase : ISoftDelete {
    public DateTime CreatedTime { get; set; } = DateTime.Now;
    public DateTime UpdatedTime { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; }
}