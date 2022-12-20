namespace StarBlog.Web.ViewModels.Response;

public interface IApiResponse {
    public int StatusCode { get; set; }
    public bool Successful { get; set; }
    public string? Message { get; set; }
}

public interface IApiResponse<T> : IApiResponse {
    public T? Data { get; set; }
}

public interface IApiErrorResponse {
    public Dictionary<string,object> ErrorData { get; set; }
}