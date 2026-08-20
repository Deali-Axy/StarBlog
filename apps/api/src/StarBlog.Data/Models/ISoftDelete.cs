namespace StarBlog.Data.Models; 

public interface ISoftDelete {
    public bool IsDeleted { get; set; }
}