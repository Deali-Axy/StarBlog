using Microsoft.AspNetCore.Identity;
using StarBlog.Api.Modules.Identity.Domain;

namespace StarBlog.Api.Modules.Identity.Security;

/// <summary>
/// 使用 ASP.NET Identity 的 PBKDF2 密码哈希。本次重建不兼容旧 SHA-256 哈希。
/// </summary>
public sealed class PasswordProtector {
    private readonly PasswordHasher<User> _hasher = new();

    /// <summary>生成不可逆密码哈希。</summary>
    public string Hash(User user, string password) => _hasher.HashPassword(user, password);

    /// <summary>校验明文密码是否匹配存储哈希。</summary>
    public bool Verify(User user, string password) =>
        _hasher.VerifyHashedPassword(user, user.Password, password) != PasswordVerificationResult.Failed;
}
