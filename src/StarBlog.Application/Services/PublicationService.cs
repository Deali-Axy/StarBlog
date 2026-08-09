using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FreeSql;
using StarBlog.Application.ViewModels.Publishing;
using StarBlog.Data.Models;

namespace StarBlog.Application.Services;

/// <summary>
/// 文章多平台投递编排服务。
///
/// 公众号使用官方草稿箱 API 直接投递；知乎和掘金没有面向普通内容创作者的稳定公开发布 API，
/// 因此为其生成可复制的 Markdown 快照和编辑器入口，而不会使用浏览器自动化或保存 Cookie。
/// 新平台只需在 <see cref="PublishAsync"/> 中增加对应适配器，不影响现有发布记录和管理 API。
/// </summary>
public sealed class PublicationService {
    private static readonly Regex ImageSourceRegex = new("""<img\b[^>]*?\bsrc\s*=\s*(["'])(?<url>.*?)\1""", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly IBaseRepository<PublicationChannel> _channelRepository;
    private readonly IBaseRepository<PostPublication> _publicationRepository;
    private readonly PostService _postService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PublicationService> _logger;

    public PublicationService(
        IBaseRepository<PublicationChannel> channelRepository,
        IBaseRepository<PostPublication> publicationRepository,
        PostService postService,
        IHttpClientFactory httpClientFactory,
        ILogger<PublicationService> logger) {
        _channelRepository = channelRepository;
        _publicationRepository = publicationRepository;
        _postService = postService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>获取渠道，永不将私密凭证返回到调用方。</summary>
    public async Task<List<PublicationChannelDto>> GetChannelsAsync() =>
        (await _channelRepository.Select.OrderBy(channel => channel.CreationTime).ToListAsync())
        .Select(PublicationChannelDto.From).ToList();

    public Task<PublicationChannel?> GetChannelAsync(string channelId) =>
        _channelRepository.Where(channel => channel.Id == channelId).FirstAsync();

    /// <summary>创建或更新渠道；更新时空密码不会覆盖现有密码。</summary>
    public async Task<PublicationChannelDto> SaveChannelAsync(string? channelId, PublicationChannelUpsertDto dto) {
        var channel = string.IsNullOrWhiteSpace(channelId) ? null : await GetChannelAsync(channelId);
        if (channel == null) {
            channel = new PublicationChannel { Id = Guid.NewGuid().ToString("N"), CreationTime = DateTime.Now };
        }

        channel.Name = dto.Name.Trim();
        channel.Platform = dto.Platform;
        channel.Enabled = dto.Enabled;
        channel.AppId = dto.AppId?.Trim();
        channel.Author = dto.Author?.Trim();
        channel.Theme = dto.Theme?.Trim();
        channel.PublishUrl = string.IsNullOrWhiteSpace(dto.PublishUrl) ? GetDefaultPublishUrl(dto.Platform) : dto.PublishUrl.Trim();
        // 凭证不应因 UI 未填写而被意外清除；删除渠道可彻底移除凭证。
        if (!string.IsNullOrWhiteSpace(dto.AppSecret)) channel.AppSecret = dto.AppSecret;
        channel.LastUpdateTime = DateTime.Now;
        await _channelRepository.InsertOrUpdateAsync(channel);
        return PublicationChannelDto.From(channel);
    }

    public Task<int> DeleteChannelAsync(string channelId) => _channelRepository.DeleteAsync(channel => channel.Id == channelId);

    public async Task<List<PostPublicationDto>> GetPublicationsAsync(string postId) =>
        (await _publicationRepository.Where(item => item.PostId == postId)
            .OrderByDescending(item => item.CreationTime).ToListAsync())
        .Select(PostPublicationDto.From).ToList();

    /// <summary>将原文按渠道要求渲染为一个不可变快照，供管理员检查后发布。</summary>
    public async Task<PostPublicationDto?> PrepareAsync(string postId, PreparePublicationDto dto) {
        var post = await _postService.GetById(postId);
        var channel = await GetChannelAsync(dto.ChannelId);
        if (post == null || channel == null || !channel.Enabled) return null;

        var publication = new PostPublication {
            Id = Guid.NewGuid().ToString("N"),
            PostId = post.Id,
            ChannelId = channel.Id,
            Platform = channel.Platform,
            Status = PublicationStatus.Prepared,
            Title = string.IsNullOrWhiteSpace(dto.Title) ? post.Title : dto.Title.Trim(),
            CoverUrl = dto.CoverUrl,
            RenderedContent = RenderForPlatform(post, channel),
            CreationTime = DateTime.Now,
            LastUpdateTime = DateTime.Now
        };
        await _publicationRepository.InsertAsync(publication);
        return PostPublicationDto.From(publication);
    }

    /// <summary>
    /// 实际投递发布快照。公众号只创建草稿，遵循微信 API 的限制，不会执行群发。
    /// </summary>
    public async Task<PostPublicationDto?> PublishAsync(string publicationId, CancellationToken cancellationToken = default) {
        var publication = await _publicationRepository.Where(item => item.Id == publicationId).FirstAsync();
        if (publication == null) return null;
        var channel = await GetChannelAsync(publication.ChannelId);
        if (channel == null || !channel.Enabled) {
            return await FailAsync(publication, "发布渠道不存在或已停用");
        }

        publication.Status = PublicationStatus.Publishing;
        publication.ErrorMessage = null;
        publication.LastUpdateTime = DateTime.Now;
        await _publicationRepository.UpdateAsync(publication);

        try {
            switch (channel.Platform) {
                case PublicationPlatform.WechatOfficialAccount:
                    var draftMediaId = await PublishWechatDraftAsync(publication, channel, cancellationToken);
                    publication.Status = PublicationStatus.Published;
                    publication.ExternalId = draftMediaId;
                    publication.ExternalUrl = "https://mp.weixin.qq.com/";
                    publication.PublishedTime = DateTime.Now;
                    break;
                case PublicationPlatform.Zhihu:
                case PublicationPlatform.Juejin:
                    // 不模拟登录、不抓取 Cookie。管理员可在 UI 复制快照并打开官方编辑器。
                    publication.Status = PublicationStatus.Prepared;
                    publication.ExternalUrl = channel.PublishUrl ?? GetDefaultPublishUrl(channel.Platform);
                    publication.ErrorMessage = "该平台没有配置可用的创作者发布 API，已生成可复制的内容快照。";
                    break;
                default:
                    return await FailAsync(publication, "该渠道尚未实现自动发布适配器");
            }
        }
        catch (Exception exception) {
            _logger.LogError(exception, "发布文章 {PostId} 到渠道 {ChannelId} 时失败", publication.PostId, channel.Id);
            return await FailAsync(publication, exception.Message);
        }

        publication.LastUpdateTime = DateTime.Now;
        await _publicationRepository.UpdateAsync(publication);
        return PostPublicationDto.From(publication);
    }

    private async Task<PostPublicationDto> FailAsync(PostPublication publication, string message) {
        publication.Status = PublicationStatus.Failed;
        publication.ErrorMessage = message;
        publication.LastUpdateTime = DateTime.Now;
        await _publicationRepository.UpdateAsync(publication);
        return PostPublicationDto.From(publication);
    }

    private static string RenderForPlatform(Post post, PublicationChannel channel) {
        if (channel.Platform is PublicationPlatform.Zhihu or PublicationPlatform.Juejin) {
            // 两个平台编辑器均接受 Markdown，保留原文便于管理员粘贴后继续编辑。
            return post.Content ?? string.Empty;
        }

        // 微信编辑器会移除 style 标签，所以所有排版样式都必须内联在容器上。
        var body = PostService.GetContentHtml(post);
        var accent = channel.Theme?.Equals("ink", StringComparison.OrdinalIgnoreCase) == true ? "#1d3557" : "#07c160";
        return $"<section style=\"font-size:16px;line-height:1.85;letter-spacing:0.03em;color:#24292f;\"><h1 style=\"font-size:26px;line-height:1.35;color:{accent};\">{System.Net.WebUtility.HtmlEncode(post.Title)}</h1>{body}</section>";
    }

    private async Task<string> PublishWechatDraftAsync(PostPublication publication, PublicationChannel channel, CancellationToken cancellationToken) {
        if (string.IsNullOrWhiteSpace(channel.AppId) || string.IsNullOrWhiteSpace(channel.AppSecret)) {
            throw new InvalidOperationException("公众号渠道缺少 AppId 或 AppSecret");
        }
        if (string.IsNullOrWhiteSpace(publication.CoverUrl)) {
            throw new InvalidOperationException("公众号草稿必须提供封面图 URL");
        }

        var client = _httpClientFactory.CreateClient();
        var token = await GetWechatAccessTokenAsync(client, channel, cancellationToken);
        var content = await ReplaceWechatImagesAsync(client, publication.RenderedContent, token, cancellationToken);
        var thumbMediaId = await UploadWechatMediaAsync(client, publication.CoverUrl, token, "thumb", cancellationToken);

        var payload = new {
            articles = new[] { new {
                title = publication.Title,
                author = channel.Author ?? string.Empty,
                content,
                thumb_media_id = thumbMediaId,
                need_open_comment = 1,
                only_fans_can_comment = 0
            } }
        };
        using var response = await client.PostAsJsonAsync($"https://api.weixin.qq.com/cgi-bin/draft/add?access_token={Uri.EscapeDataString(token)}", payload, cancellationToken);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        if (!response.IsSuccessStatusCode || !document.RootElement.TryGetProperty("media_id", out var mediaId)) {
            throw new InvalidOperationException(GetWechatError(document.RootElement));
        }
        return mediaId.GetString() ?? throw new InvalidOperationException("微信草稿接口未返回 media_id");
    }

    private static async Task<string> GetWechatAccessTokenAsync(HttpClient client, PublicationChannel channel, CancellationToken cancellationToken) {
        var url = $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={Uri.EscapeDataString(channel.AppId!)}&secret={Uri.EscapeDataString(channel.AppSecret!)}";
        using var response = await client.GetAsync(url, cancellationToken);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        if (!response.IsSuccessStatusCode || !document.RootElement.TryGetProperty("access_token", out var token)) {
            throw new InvalidOperationException(GetWechatError(document.RootElement));
        }
        return token.GetString() ?? throw new InvalidOperationException("微信接口未返回 access_token");
    }

    private static async Task<string> ReplaceWechatImagesAsync(HttpClient client, string html, string token, CancellationToken cancellationToken) {
        var matches = ImageSourceRegex.Matches(html);
        if (matches.Count == 0) return html;
        var result = new StringBuilder(html);
        // 倒序替换避免前一次插入的 URL 改变之后匹配项的索引。
        for (var index = matches.Count - 1; index >= 0; index--) {
            var match = matches[index];
            var source = match.Groups["url"].Value;
            if (!Uri.TryCreate(source, UriKind.Absolute, out _)) continue;
            var wechatUrl = await UploadWechatMediaAsync(client, source, token, "image", cancellationToken);
            var urlGroup = match.Groups["url"];
            result.Remove(urlGroup.Index, urlGroup.Length);
            result.Insert(urlGroup.Index, wechatUrl);
        }
        return result.ToString();
    }

    private static async Task<string> UploadWechatMediaAsync(HttpClient client, string sourceUrl, string token, string type, CancellationToken cancellationToken) {
        using var source = await client.GetAsync(sourceUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        source.EnsureSuccessStatusCode();
        await using var stream = await source.Content.ReadAsStreamAsync(cancellationToken);
        using var form = new MultipartFormDataContent();
        using var file = new StreamContent(stream);
        file.Headers.ContentType = source.Content.Headers.ContentType ?? new MediaTypeHeaderValue("application/octet-stream");
        form.Add(file, "media", "starblog-image.jpg");
        var endpoint = type == "image"
            ? $"https://api.weixin.qq.com/cgi-bin/media/uploadimg?access_token={Uri.EscapeDataString(token)}"
            : $"https://api.weixin.qq.com/cgi-bin/material/add_material?access_token={Uri.EscapeDataString(token)}&type=thumb";
        using var response = await client.PostAsync(endpoint, form, cancellationToken);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var propertyName = type == "image" ? "url" : "media_id";
        if (!response.IsSuccessStatusCode || !document.RootElement.TryGetProperty(propertyName, out var value)) {
            throw new InvalidOperationException(GetWechatError(document.RootElement));
        }
        return value.GetString() ?? throw new InvalidOperationException($"微信图片接口未返回 {propertyName}");
    }

    private static string GetWechatError(JsonElement response) {
        if (response.TryGetProperty("errmsg", out var message)) return $"微信接口错误: {message.GetString()}";
        return "微信接口返回了未知错误";
    }

    private static string GetDefaultPublishUrl(PublicationPlatform platform) => platform switch {
        PublicationPlatform.Zhihu => "https://zhuanlan.zhihu.com/write",
        PublicationPlatform.Juejin => "https://juejin.cn/editor/drafts/new?v=2",
        PublicationPlatform.WechatOfficialAccount => "https://mp.weixin.qq.com/",
        _ => string.Empty
    };
}
