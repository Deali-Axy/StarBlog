using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StarBlog.Api.Hosting.Authentication;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Modules.Configuration;
using StarBlog.Api.Modules.Identity.Domain;
using StarBlog.Api.Modules.Identity.Security;

namespace StarBlog.Api.Modules.Identity;

/// <summary>
/// 登录、JWT 签发与首次初始化。
/// </summary>
public sealed class IdentityOperations {
    public const string ClaimUserId = "user_id";
    public const string ClaimUserName = "user_name";

    private readonly StarBlogDbContext _db;
    private readonly PasswordProtector _passwordProtector;
    private readonly AuthOptions _auth;
    private readonly ISiteConfiguration _siteConfiguration;

    public IdentityOperations(
        StarBlogDbContext db,
        PasswordProtector passwordProtector,
        IOptions<AuthOptions> auth,
        ISiteConfiguration siteConfiguration) {
        _db = db;
        _passwordProtector = passwordProtector;
        _auth = auth.Value;
        _siteConfiguration = siteConfiguration;
    }

    /// <summary>用户表是否已有管理员。</summary>
    public Task<bool> IsInitializedAsync(CancellationToken cancellationToken) =>
        _db.Users.AnyAsync(cancellationToken);

    /// <summary>按用户名查找用户。</summary>
    public Task<User?> FindByNameAsync(string name, CancellationToken cancellationToken) =>
        _db.Users.FirstOrDefaultAsync(user => user.Name == name, cancellationToken);

    /// <summary>校验密码并签发 7 天有效的 JWT。</summary>
    public LoginTokenResponse? Authenticate(User user, string password) {
        if (!_passwordProtector.Verify(user, password)) return null;
        return GenerateLoginToken(user);
    }

    /// <summary>从 JWT 声明还原当前用户，不访问数据库。</summary>
    public static CurrentUserResponse? GetCurrentUser(ClaimsPrincipal principal) {
        var userId = principal.FindFirst(ClaimUserId)?.Value;
        var userName = principal.FindFirst(ClaimUserName)?.Value;
        if (userId == null || userName == null) return null;
        return new CurrentUserResponse { Id = userId, Name = userName };
    }

    /// <summary>空库时创建首个管理员，并写入 host / default_render / is_init。</summary>
    public async Task<string?> InitializeAsync(InitializeSiteRequest request, CancellationToken cancellationToken) {
        if (await _db.Users.AnyAsync(cancellationToken)) {
            return "站点已经完成初始化";
        }

        var user = new User {
            Id = Guid.NewGuid().ToString("N"),
            Name = request.Username.Trim()
        };
        user.Password = _passwordProtector.Hash(user, request.Password);
        _db.Users.Add(user);

        await _siteConfiguration.SetAsync("host", request.Host.TrimEnd('/'), "站点地址", cancellationToken);
        await _siteConfiguration.SetAsync("default_render", request.DefaultRender, "默认渲染目标", cancellationToken);
        await _siteConfiguration.SetAsync("is_init", "true", "站点是否已完成初始化", cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return null;
    }

    /// <summary>签发 HMAC-SHA256 JWT。</summary>
    public LoginTokenResponse GenerateLoginToken(User user) {
        var claims = new List<Claim> {
            new(ClaimUserId, user.Id),
            new(ClaimUserName, user.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_auth.Jwt.Key));
        var token = new JwtSecurityToken(
            issuer: _auth.Jwt.Issuer,
            audience: _auth.Jwt.Audience,
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new LoginTokenResponse {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = TimeZoneInfo.ConvertTimeFromUtc(token.ValidTo, TimeZoneInfo.Local)
        };
    }
}
