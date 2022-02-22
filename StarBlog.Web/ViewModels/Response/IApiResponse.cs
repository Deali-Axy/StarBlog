namespace StarBlog.Web.ViewModels.Response;

public interface IApiResponse {
    public bool Successful { get; set; }
    public string? Message { get; set; }
}

public interface IApiResponse<T> : IApiResponse {
    public T? Data { get; set; }
}