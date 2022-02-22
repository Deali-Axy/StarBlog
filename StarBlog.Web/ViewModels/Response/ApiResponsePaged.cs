namespace StarBlog.Web.ViewModels.Response;

public class ApiResponsePaged<T> : ApiResponse<List<T>> where T : class {
    public PaginationMetadata? Pagination { get; set; }
}