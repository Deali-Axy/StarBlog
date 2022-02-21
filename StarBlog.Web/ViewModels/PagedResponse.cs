namespace StarBlog.Web.ViewModels;

public class PagedResponse<T> where T : class {
    public bool Successful { get; set; }
    public string? Message { get; set; }
    public PaginationMetadata? Pagination { get; set; }
    public List<T> Data { get; set; }
}