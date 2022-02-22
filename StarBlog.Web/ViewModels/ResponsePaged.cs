namespace StarBlog.Web.ViewModels;

public class ResponsePaged<T> : Response<List<T>> where T : class {
    public PaginationMetadata? Pagination { get; set; }
}