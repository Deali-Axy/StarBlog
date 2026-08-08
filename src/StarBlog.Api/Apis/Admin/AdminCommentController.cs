using CodeLab.Share.Extensions;
using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Application.Criteria;
using StarBlog.Application.Services;
using StarBlog.Application.ViewModels.Comments;
using StarBlog.Data.Models;

namespace StarBlog.Api.Apis.Admin;

/// <summary>
/// 管理端评论工作台。
/// 原 <c>/Api/Comment</c> 保留给前台读取和提交；此控制器提供只允许管理员访问的独立入口。
/// </summary>
[Authorize]
[ApiController]
[Route("Api/Admin/Comments")]
[ApiExplorerSettings(GroupName = Extensions.ApiGroups.Admin)]
public sealed class AdminCommentController : ControllerBase {
    private readonly CommentService _commentService;

    public AdminCommentController(CommentService commentService) => _commentService = commentService;

    /// <summary>按管理员口径返回评论，包括待审核与不可见评论。</summary>
    [HttpGet]
    public async Task<ApiResponsePaged<Comment>> GetList([FromQuery] CommentQueryParameters parameters) {
        var (data, pagination) = await _commentService.GetPagedList(parameters, true, true);
        return new ApiResponsePaged<Comment>(data, pagination);
    }

    /// <summary>审核通过一条评论。</summary>
    [HttpPost("{id}/accept")]
    public async Task<ApiResponse<Comment>> Accept(string id, [FromBody] CommentAcceptDto dto) {
        var comment = await _commentService.GetById(id);
        return comment == null ? ApiResponse.NotFound() : new ApiResponse<Comment>(await _commentService.Accept(comment, dto.Reason));
    }

    /// <summary>审核拒绝一条评论。</summary>
    [HttpPost("{id}/reject")]
    public async Task<ApiResponse<Comment>> Reject(string id, [FromBody] CommentRejectDto dto) {
        var comment = await _commentService.GetById(id);
        return comment == null ? ApiResponse.NotFound() : new ApiResponse<Comment>(await _commentService.Reject(comment, dto.Reason));
    }
}
