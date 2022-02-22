namespace StarBlog.Web.ViewModels.Response;

public class ApiResponse {
    public bool Successful { get; set; } = true;
    public string? Message { get; set; }

    public static ApiResponse Ok(HttpResponse httpResponse, string? message = null) {
        httpResponse.StatusCode = StatusCodes.Status200OK;
        return new ApiResponse {Successful = true, Message = message};
    }

    public static ApiResponse NotFound(HttpResponse httpResponse) {
        httpResponse.StatusCode = StatusCodes.Status404NotFound;
        return new ApiResponse {Successful = false, Message = "Not found."};
    }
}

public class ApiResponse<T> : ApiResponse where T : class {
    public T? Data { get; set; }

    public new static ApiResponse<T> NotFound(HttpResponse httpResponse) {
        httpResponse.StatusCode = StatusCodes.Status404NotFound;
        return new ApiResponse<T> {Successful = false, Message = "Not found."};
    }
}