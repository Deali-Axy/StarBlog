namespace StarBlog.Web.ViewModels;

public class Response {
    public bool Successful { get; set; } = true;
    public string? Message { get; set; }
}

public class Response<T> : Response where T : class {
    public T? Data { get; set; }
}