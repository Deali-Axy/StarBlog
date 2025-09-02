using StarBlog.Data.Models;

namespace StarBlog.Web.ViewModels.Search; 

public class SearchResultViewModel {
    public string Keyword { get; set; }
    public List<SearchPost> SearchPosts { get; set; }
}