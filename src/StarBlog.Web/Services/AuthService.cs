using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FreeSql;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StarBlog.Data.Models;
using StarBlog.Web.Models.Config;
using StarBlog.Web.ViewModels.Auth;

namespace StarBlog.Web.Services;

public class AuthService {
    private readonly Auth _auth;
    private readonly IBaseRepository<User> _userRepo;

    private const string ClaimUserId = "user_id";
    private const string ClaimUserName = "user_name";

    public AuthService(IOptions<Auth> options, IBaseRepository<User> userRepo) {
        _auth = options.Value;
        _userRepo = userRepo;
    }

    public LoginToken GenerateLoginToken(User user) {
        var claims = new List<Claim> {
            new(ClaimUserId, user.Id), // User.Identity.Name
            new(ClaimUserName, user.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_auth.Jwt.Key));
        // todo 使用非对称加密 jwt (RSA)
        var signCredential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwtToken = new JwtSecurityToken(
            issuer: _auth.Jwt.Issuer,
            audience: _auth.Jwt.Audience,
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: signCredential
        );

        // todo 尝试使用 jose-jwt 生成 jwt
        return new LoginToken {
            Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
            Expiration = TimeZoneInfo.ConvertTimeFromUtc(jwtToken.ValidTo, TimeZoneInfo.Local)
        };
    }

    public async Task<User?> GetUserById(string userId) {
        return await _userRepo.Where(a => a.Id == userId).FirstAsync();
    }

    public async Task<User?> GetUserByName(string name) {
        return await _userRepo.Where(a => a.Name == name).FirstAsync();
    }

    public User? GetUser(ClaimsPrincipal userClaim) {
        var userId = userClaim.FindFirstValue(ClaimUserId);
        var userName = userClaim.FindFirstValue(ClaimUserName);
        if (userId == null || userName == null) return null;
        return new User { Id = userId, Name = userName };
    }
}