using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StarBlog.Web.Filters;

public class ResponseWrapperFilter : IAsyncResultFilter {
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

                objectResult.Value = wrapperResp;
                objectResult.DeclaredType = wrapperResp.GetType();
            }
        }

        await next();
    }
}