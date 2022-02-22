using StarBlog.Data.Models;

namespace StarBlog.Web.Dtos; 

public class PostListDto {
    public string Id { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Path { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastUpdateTime { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    public string Categories { get; set; }
}