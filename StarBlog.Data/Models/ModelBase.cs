namespace StarBlog.Data.Models;

public abstract class ModelBase : ISoftDelete {
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedTime { get; set; }
    public bool IsDeleted { get; set; }
}