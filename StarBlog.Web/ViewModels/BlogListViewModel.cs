using StarBlog.Data.Models;
using X.PagedList;

namespace StarBlog.Web.ViewModels; 

public class BlogListViewModel {
    public Category CurrentCategory { get; set; }
    public int CurrentCategoryId { get; set; }
    public IPagedList<Post> Posts { get; set; }
    public List<Category> Categories { get; set; }
}