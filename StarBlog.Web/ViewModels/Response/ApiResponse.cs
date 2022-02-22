using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StarBlog.Web.ViewModels.Response;

public class ApiResponse<T> : IApiResponse<T> {
    public ApiResponse() {
    }

    public ApiResponse(T? data) {
        Data = data;
    }

    public int StatusCode { get; set; } = 200;
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
    public int StatusCode { get; set; } = 200;
    public bool Successful { get; set; }
    public string? Message { get; set; }

    public static ApiResponse NoContent(HttpResponse httpResponse, string message = "NoContent") {
        httpResponse.StatusCode = StatusCodes.Status204NoContent;
        return new ApiResponse {
            StatusCode = httpResponse.StatusCode,
            Successful = true, Message = message
        };
    }

    public static ApiResponse Ok(HttpResponse httpResponse, string message = "Ok") {
        httpResponse.StatusCode = StatusCodes.Status200OK;
        return new ApiResponse {
            StatusCode = httpResponse.StatusCode,
            Successful = true, Message = message
        };
    }

    public static ApiResponse Unauthorized(HttpResponse httpResponse, string message = "Unauthorized") {
        httpResponse.StatusCode = StatusCodes.Status401Unauthorized;
        return new ApiResponse {
            StatusCode = httpResponse.StatusCode,
            Successful = false, Message = message
        };
    }

    public static ApiResponse NotFound(HttpResponse httpResponse, string message = "NotFound") {
        httpResponse.StatusCode = StatusCodes.Status404NotFound;
        return new ApiResponse {
            StatusCode = httpResponse.StatusCode,
            Successful = false, Message = message
        };
    }

    public static ApiResponse BadRequest(HttpResponse httpResponse, string message = "BadRequest") {
        httpResponse.StatusCode = StatusCodes.Status400BadRequest;
        return new ApiResponse {
            StatusCode = httpResponse.StatusCode,
            Successful = false, Message = message
        };
    }

    public static ApiResponse<SerializableError> BadRequest(HttpResponse httpResponse
        , string message, ModelStateDictionary modelState) {
        httpResponse.StatusCode = StatusCodes.Status400BadRequest;
        return new ApiResponse<SerializableError> {
            StatusCode = httpResponse.StatusCode,
            Successful = false, Message = message, Data = new SerializableError(modelState)
        };
    }

    public static ApiResponse Error(HttpResponse httpResponse, string message = "Error") {
        httpResponse.StatusCode = StatusCodes.Status500InternalServerError;
        return new ApiResponse {
            StatusCode = httpResponse.StatusCode,
            Successful = false, Message = message
        };
    }
}