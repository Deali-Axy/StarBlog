using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;
using StarBlog.Web.ViewModels.Response;

namespace StarBlog.Web.Controllers;

/// <summary>
/// 认证
/// </summary>
[ApiController]
[Route("Api/[controller]")]
public class AuthController : ControllerBase {
    private readonly AuthService _authService;

    public AuthController(AuthService authService) {
        _authService = authService;
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="loginUser"></param>
    /// <returns></returns>
    [HttpPost]
    public ApiResponse<LoginToken> Login(LoginUser loginUser) {
        var user = _authService.GetUserByName(loginUser.Username);
        if (user == null) return ApiResponse.NotFound(Response);
        if (loginUser.Password != user.Password) return ApiResponse.Unauthorized(Response);
        return new ApiResponse<LoginToken>(_authService.GenerateLoginToken(user));
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    public ActionResult<User> GetUser() {
        var user = _authService.GetUser(User);
        if (user == null) return NotFound();
        return user;
    }
}