namespace StarBlog.Application.ViewModels.Auth;

public class LoginToken {
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}
