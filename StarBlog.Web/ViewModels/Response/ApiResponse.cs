namespace StarBlog.Web.ViewModels.Response;

public class ApiResponse<T> : IApiResponse<T> {
    public bool Successful { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    /// <summary>
    /// 实现将 <see cref="ApiResponse"/> 隐式转换为 <see cref="ApiResponse{T}"/>
    /// </summary>
    /// <param name="apiResponse"><see cref="ApiResponse"/></param>
    /// <returns></returns>
    public static implicit operator ApiResponse<T>(ApiResponse apiResponse) {
        return new ApiResponse<T> {
            Successful = apiResponse.Successful,
            Message = apiResponse.Message
        };
    }
}

public class ApiResponse : IApiResponse {
    public bool Successful { get; set; }
    public string? Message { get; set; }

    public static ApiResponse Ok(HttpResponse httpResponse, string message = "Ok") {
        httpResponse.StatusCode = StatusCodes.Status200OK;
        return new ApiResponse { Successful = true, Message = message };
    }

    public static ApiResponse Unauthorized(HttpResponse httpResponse, string message = "Unauthorized") {
        httpResponse.StatusCode = StatusCodes.Status401Unauthorized;
        return new ApiResponse { Successful = false, Message = message };
    }

    public static ApiResponse NotFound(HttpResponse httpResponse, string message = "NotFound") {
        httpResponse.StatusCode = StatusCodes.Status404NotFound;
        return new ApiResponse { Successful = false, Message = message };
    }

    public static ApiResponse BadRequest(HttpResponse httpResponse, string message = "BadRequest") {
        httpResponse.StatusCode = StatusCodes.Status400BadRequest;
        return new ApiResponse { Successful = false, Message = message };
    }
}