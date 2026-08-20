namespace StarBlog.Api.Modules.Identity.Domain;

/// <summary>
/// 站点管理员。当前不区分角色，持有有效 JWT 即可访问管理端。
/// </summary>
public sealed class User {
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
