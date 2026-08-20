namespace StarBlog.Application.Models.Config; 

public class Auth {
    public Jwt Jwt { get; set; }
}

public class Jwt {
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Key { get; set; }
}