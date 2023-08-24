using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Share.Extensions;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Auth;

namespace StarBlog.Web.Apis;

/// <summary>
/// 认证
/// </summary>
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Auth)]
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
    [Route("")]
    [Route("[action]")]
    [ProducesResponseType(typeof(ApiResponse<LoginToken>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> Login(LoginUser loginUser) {
        var user = await _authService.GetUserByName(loginUser.Username);
        if (user == null) return ApiResponse.Unauthorized("用户名或密码错误");
        if (loginUser.Password.ToSHA256() != user.Password) return ApiResponse.Unauthorized("用户名或密码错误");
        return ApiResponse.Ok(_authService.GenerateLoginToken(user));
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    [Route("")]
    [Route("[action]")]
    public ApiResponse<User> GetUser() {
        var user = _authService.GetUser(User);
        if (user == null) return ApiResponse.NotFound("找不到用户资料");
        return new ApiResponse<User>(user);
    }
}