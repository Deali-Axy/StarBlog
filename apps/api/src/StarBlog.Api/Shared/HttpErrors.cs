namespace StarBlog.Api.Shared;

/// <summary>
/// 将业务错误映射为 RFC 7807 ProblemDetails，供 Minimal API endpoint 统一返回。
/// </summary>
public static class HttpErrors {
    /// <summary>返回 400 Bad Request。</summary>
    public static IResult BadRequest(string detail) =>
        Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Bad Request", detail: detail);

    /// <summary>返回 401 Unauthorized。</summary>
    public static IResult Unauthorized(string detail) =>
        Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized", detail: detail);

    /// <summary>返回 404 Not Found。</summary>
    public static IResult NotFound(string detail) =>
        Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not Found", detail: detail);

    /// <summary>返回 409 Conflict。</summary>
    public static IResult Conflict(string detail) =>
        Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflict", detail: detail);

    /// <summary>返回 422 Unprocessable Entity。</summary>
    public static IResult Unprocessable(string detail) =>
        Results.Problem(statusCode: StatusCodes.Status422UnprocessableEntity, title: "Unprocessable Entity", detail: detail);
}
