using StarBlog.Web.Extensions;
using X.PagedList;

namespace StarBlog.Web.ViewModels.Response;

public class ApiResponsePaged<T> : ApiResponse<List<T>> where T : class {
    public ApiResponsePaged() {
    }

    public ApiResponsePaged(IPagedList<T> pagedList) {
        Data = pagedList.ToList();
        Pagination = pagedList.ToPaginationMetadata();
    }

    public PaginationMetadata? Pagination { get; set; }
}