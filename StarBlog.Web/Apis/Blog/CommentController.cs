using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Comments;
using StarBlog.Web.ViewModels.QueryFilters;

namespace StarBlog.Web.Apis.Blog;

[Route("Api/[controller]")]
[ApiController]
public class CommentController : ControllerBase {
    private readonly CommentService _commentService;

    public CommentController(CommentService commentService) {
        _commentService = commentService;
    }

    /// <summary>
    /// 获取分页评论
    /// </summary>
    [HttpGet]
    public async Task<ApiResponsePaged<Comment>> GetPagedList([FromQuery] CommentQueryParameters @params) {
        var (data, meta) = await _commentService.GetPagedList(@params);
        return new ApiResponsePaged<Comment>(data, meta);
    }

    /// <summary>
    /// 获取邮件验证码
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> GetEmailOtp(string email) {
        if (!CommentService.IsValidEmail(email)) {
            return ApiResponse.BadRequest("提供的邮箱地址无效");
        }

        var result = await _commentService.GenerateOtp(email);
        return result
            ? ApiResponse.Ok("发送邮件验证码成功，五分钟内有效")
            : ApiResponse.BadRequest("上一个验证码还在有效期内，请勿重复请求验证码");
    }

    [HttpPost]
    public async Task<ApiResponse<Comment>> Add(CommentCreationDto dto) {
        if (!CommentService.IsValidEmail(dto.Email)) {
            return ApiResponse.BadRequest("提供的邮箱地址无效");
        }

        if (!await _commentService.VerifyOtp(dto.Email, dto.EmailOtp)) {
            return ApiResponse.BadRequest("验证码无效");
        }

        var anonymousUser = await _commentService.GetOrCreateAnonymousUser(
            dto.UserName, dto.Email, dto.Url,
            HttpContext.GetRemoteIPAddress()?.ToString().Split(":")?.Last()
        );

        var comment = new Comment {
            PostId = dto.PostId,
            AnonymousUserId = anonymousUser.Id,
            UserAgent = Request.Headers.UserAgent,
            Content = dto.Content
        };
        return new ApiResponse<Comment>(await _commentService.Add(comment)) {
            Message = "评论已提交，将在审核通过后展示"
        };
    }
}