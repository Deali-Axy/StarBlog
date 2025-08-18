using StarBlog.Data.Models;

namespace StarBlog.Web.ViewModels; 

public class SearchResultViewModel {
    public string Keyword { get; set; }
    public List<Post> Posts { get; set; }
}