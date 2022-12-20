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
    public bool Successful { get; set; } = true;
    public string? Message { get; set; }

    public T? Data { get; set; }

    /// <summary>
    /// 实现将 <see cref="ApiResponse"/> 隐式转换为 <see cref="ApiResponse{T}"/>
    /// </summary>
    /// <param name="apiResponse"><see cref="ApiResponse"/></param>
    /// <returns></returns>
    public static implicit operator ApiResponse<T>(ApiResponse apiResponse) {
        return new ApiResponse<T> {
            StatusCode = apiResponse.StatusCode,
            Successful = apiResponse.Successful,
            Message = apiResponse.Message
        };
    }
}

public class ApiResponse : IApiResponse, IApiErrorResponse {
    public int StatusCode { get; set; } = 200;
    public bool Successful { get; set; } = true;
    public string? Message { get; set; }
    public object? Data { get; set; }

    /// <summary>
    /// 可序列化的错误
    /// <para>用于保存模型验证失败的错误信息</para>
    /// </summary>
    public Dictionary<string,object>? ErrorData { get; set; }

    public ApiResponse() {
    }

    public ApiResponse(object data) {
        Data = data;
    }

    public static ApiResponse NoContent(string message = "NoContent") {
        return new ApiResponse {
            StatusCode = StatusCodes.Status204NoContent,
            Successful = true, Message = message
        };
    }

    public static ApiResponse Ok(string message = "Ok") {
        return new ApiResponse {
            StatusCode = StatusCodes.Status200OK,
            Successful = true, Message = message
        };
    }

    public static ApiResponse Ok(object data, string message = "Ok") {
        return new ApiResponse {
            StatusCode = StatusCodes.Status200OK,
            Successful = true, Message = message,
            Data = data
        };
    }

    public static ApiResponse Unauthorized(string message = "Unauthorized") {
        return new ApiResponse {
            StatusCode = StatusCodes.Status401Unauthorized,
            Successful = false, Message = message
        };
    }

    public static ApiResponse NotFound(string message = "NotFound") {
        return new ApiResponse {
            StatusCode = StatusCodes.Status404NotFound,
            Successful = false, Message = message
        };
    }

    public static ApiResponse BadRequest(string message = "BadRequest") {
        return new ApiResponse {
            StatusCode = StatusCodes.Status400BadRequest,
            Successful = false, Message = message
        };
    }

    public static ApiResponse BadRequest(ModelStateDictionary modelState, string message = "ModelState is not valid.") {
        return new ApiResponse {
            StatusCode = StatusCodes.Status400BadRequest,
            Successful = false, Message = message,
            ErrorData = new SerializableError(modelState)
        };
    }

    public static ApiResponse Error(string message = "Error", Exception? exception = null) {
        object? data = null;
        if (exception != null) {
            data = new {
                exception.Message,
                exception.Data
            };
        }

        return new ApiResponse {
            StatusCode = StatusCodes.Status500InternalServerError,
            Successful = false,
            Message = message,
            Data = data
        };
    }
}