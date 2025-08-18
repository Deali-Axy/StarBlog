using StarBlog.Data.Models;
using X.PagedList;

namespace StarBlog.Web.ViewModels.Photography; 

public class PhotographyViewModel {
    public IPagedList<Photo> Photos { get; set; }
}