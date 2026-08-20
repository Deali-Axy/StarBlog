namespace StarBlog.Api.Hosting.Authentication;

/// <summary>
/// JWT 签发与校验配置。
/// </summary>
public sealed class AuthOptions {
    public const string SectionName = "Auth";
    public JwtOptions Jwt { get; set; } = new();
}

/// <summary>对称密钥 JWT 参数。</summary>
public sealed class JwtOptions {
    public string Issuer { get; set; } = "starblog";
    public string Audience { get; set; } = "starblog-admin-ui";
    public string Key { get; set; } = string.Empty;
}
