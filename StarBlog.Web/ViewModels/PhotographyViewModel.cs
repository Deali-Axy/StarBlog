using StarBlog.Data.Models;
using X.PagedList;

namespace StarBlog.Web.ViewModels; 

public class PhotographyViewModel {
    public IPagedList<Photo> Photos { get; set; }
}