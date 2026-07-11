namespace StarBlog.Web.Models.Config; 

public class Auth {
    public Jwt Jwt { get; set; } = new();
}

public class Jwt {
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}
