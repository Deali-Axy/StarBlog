using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.WebEncoders.Testing;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels.Categories;
using X.PagedList;

namespace StarBlog.Web.ViewModels.Blog;

public class BlogListViewModel {
    public string SortType { get; set; }
    public string SortBy { get; set; }
    public Category CurrentCategory { get; set; }
    public int CurrentCategoryId { get; set; }
    public IPagedList<Post> Posts { get; set; }
    public List<Category> Categories { get; set; }
    public List<CategoryNode>? CategoryNodes { get; set; }

    public string CategoryNodesJson => JsonSerializer.Serialize(
        CategoryNodes,
        new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }
    );
}