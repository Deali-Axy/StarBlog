using System.Text;
using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StarBlog.Api.Filters;

public class ResponseWrapperFilter : IActionFilter, IAsyncResultFilter {
    private readonly ILogger<ResponseWrapperFilter> _logger;

    public ResponseWrapperFilter(ILogger<ResponseWrapperFilter> logger) {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context) {
    }

    public void OnActionExecuted(ActionExecutedContext context) {
        if (context.Exception != null) {
            _logger.LogError("Response Error: {ExceptionStackTrace}", context.Exception.StackTrace);
            context.Result = new ObjectResult(new ApiResponse {
                StatusCode = StatusCodes.Status500InternalServerError,
                Successful = false,
                Data = context.Exception.Data,
                Message = context.Exception.Message
            });
            context.ExceptionHandled = true;
        }
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next) {
        if (context.Result is ObjectResult objectResult) {
            if (objectResult.Value is IApiResponse apiResponse) {
                objectResult.StatusCode = apiResponse.StatusCode;
                context.HttpContext.Response.StatusCode = apiResponse.StatusCode;
            }
            else {
                var statusCode = objectResult.StatusCode ?? context.HttpContext.Response.StatusCode;

                var wrapperResp = new ApiResponse<object> {
                    StatusCode = statusCode,
                    Successful = statusCode is >= 200 and < 400,
                    Data = objectResult.Value,
                };

                if (objectResult.Value is HttpValidationProblemDetails problemDetails) {
                    var sb = new StringBuilder();
                    foreach (var (key, value) in problemDetails.Errors) {
                        sb.Append($"{key}: {string.Join(',', value)}；");
                    }

                    wrapperResp.Message = sb.ToString();
                }

                objectResult.Value = wrapperResp;
                objectResult.DeclaredType = wrapperResp.GetType();
            }
        }

        await next();
    }
}