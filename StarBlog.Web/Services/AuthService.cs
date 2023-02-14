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
    private readonly SecuritySettings _securitySettings;
    private readonly IBaseRepository<User> _userRepo;

    public AuthService(IOptions<SecuritySettings> options, IBaseRepository<User> userRepo) {
        _securitySettings = options.Value;
        _userRepo = userRepo;
    }

    public LoginToken GenerateLoginToken(User user) {
        var claims = new List<Claim> {
            new("username", user.Name),
            new(JwtRegisteredClaimNames.Name, user.Id), // User.Identity.Name
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securitySettings.Token.Key));
        var signCredential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwtToken = new JwtSecurityToken(
            issuer: _securitySettings.Token.Issuer,
            audience: _securitySettings.Token.Audience,
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: signCredential);

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
        var userId = userClaim.Identity?.Name;
        var userName = userClaim.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
        if (userId == null || userName == null) return null;
        return new User { Id = userId, Name = userName };
    }
}