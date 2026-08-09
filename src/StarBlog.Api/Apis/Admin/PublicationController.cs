using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Api.Extensions;
using StarBlog.Application.Services;
using StarBlog.Application.ViewModels.Publishing;

namespace StarBlog.Api.Apis.Admin;

/// <summary>
/// 多平台发布管理 API。
/// 先创建渠道，再为文章生成发布快照，最后按渠道能力推送或复制到官方编辑器。
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/admin/publication-channels")]
[ApiExplorerSettings(GroupName = ApiGroups.Admin)]
public sealed class PublicationController : ControllerBase {
    private readonly PublicationService _publicationService;

    public PublicationController(PublicationService publicationService) => _publicationService = publicationService;

    /// <summary>列出已配置的发布渠道（不会泄露密钥）。</summary>
    [HttpGet]
    public async Task<ApiResponse<List<PublicationChannelDto>>> GetChannels() =>
        new(await _publicationService.GetChannelsAsync());

    /// <summary>新建发布渠道。</summary>
    [HttpPost]
    public async Task<ApiResponse<PublicationChannelDto>> CreateChannel([FromBody] PublicationChannelUpsertDto dto) {
        if (!ModelState.IsValid) return ApiResponse.BadRequest(ModelState);
        return new ApiResponse<PublicationChannelDto>(await _publicationService.SaveChannelAsync(null, dto));
    }

    /// <summary>更新发布渠道；未传 AppSecret 时保留当前密钥。</summary>
    [HttpPut("{channelId}")]
    public async Task<ApiResponse<PublicationChannelDto>> UpdateChannel(string channelId, [FromBody] PublicationChannelUpsertDto dto) {
        if (!ModelState.IsValid) return ApiResponse.BadRequest(ModelState);
        if (await _publicationService.GetChannelAsync(channelId) == null) return ApiResponse.NotFound("发布渠道不存在");
        return new ApiResponse<PublicationChannelDto>(await _publicationService.SaveChannelAsync(channelId, dto));
    }

    /// <summary>删除渠道及其凭证；已有投递记录会保留以便审计。</summary>
    [HttpDelete("{channelId}")]
    public async Task<ApiResponse> DeleteChannel(string channelId) {
        var rows = await _publicationService.DeleteChannelAsync(channelId);
        return rows == 0 ? ApiResponse.NotFound("发布渠道不存在") : ApiResponse.Ok("发布渠道已删除");
    }

    /// <summary>查看一篇文章的全部投递历史。</summary>
    [HttpGet("/api/v1/admin/posts/{postId}/publications")]
    public async Task<ApiResponse<List<PostPublicationDto>>> GetPostPublications(string postId) =>
        new(await _publicationService.GetPublicationsAsync(postId));

    /// <summary>按所选渠道生成一个可预览、可重复发布的内容快照。</summary>
    [HttpPost("/api/v1/admin/posts/{postId}/publications")]
    public async Task<ApiResponse<PostPublicationDto>> Prepare(string postId, [FromBody] PreparePublicationDto dto) {
        if (!ModelState.IsValid) return ApiResponse.BadRequest(ModelState);
        var publication = await _publicationService.PrepareAsync(postId, dto);
        return publication == null ? ApiResponse.BadRequest("文章不存在，或发布渠道不存在/已停用") : new ApiResponse<PostPublicationDto>(publication);
    }

    /// <summary>
    /// 执行投递。微信公众号会进入草稿箱；知乎、掘金返回可复制快照和官方编辑器链接。
    /// </summary>
    [HttpPost("/api/v1/admin/publications/{publicationId}/deliveries")]
    public async Task<ApiResponse<PostPublicationDto>> Publish(string publicationId, CancellationToken cancellationToken) {
        var publication = await _publicationService.PublishAsync(publicationId, cancellationToken);
        return publication == null ? ApiResponse.NotFound("发布记录不存在") : new ApiResponse<PostPublicationDto>(publication);
    }
}
