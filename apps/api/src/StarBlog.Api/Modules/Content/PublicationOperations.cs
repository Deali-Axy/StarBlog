using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Infrastructure.Time;
using StarBlog.Api.Modules.Content.Domain;
using StarBlog.Api.Modules.Content.Markdown;

namespace StarBlog.Api.Modules.Content;

/// <summary>
/// 多平台发布：公众号走官方草稿箱 API，知乎/掘金只生成可复制快照。
/// </summary>
public sealed class PublicationOperations {
    private static readonly Regex ImageSourceRegex = new("""<img\b[^>]*?\bsrc\s*=\s*(["'])(?<url>.*?)\1""", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly StarBlogDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IClock _clock;
    private readonly ILogger<PublicationOperations> _logger;

    public PublicationOperations(
        StarBlogDbContext db,
        IHttpClientFactory httpClientFactory,
        IClock clock,
        ILogger<PublicationOperations> logger) {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _clock = clock;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PublicationChannelResponse>> GetChannelsAsync(CancellationToken cancellationToken) {
        var channels = await _db.PublicationChannels.AsNoTracking().OrderBy(item => item.CreationTime).ToListAsync(cancellationToken);
        return channels.Select(PublicationChannelResponse.From).ToList();
    }

    /// <summary>创建或更新渠道；空 AppSecret 不会覆盖已存密钥。</summary>
    public async Task<PublicationChannelResponse> SaveChannelAsync(string? channelId, PublicationChannelUpsertRequest request, CancellationToken cancellationToken) {
        var channel = string.IsNullOrWhiteSpace(channelId)
            ? null
            : await _db.PublicationChannels.FirstOrDefaultAsync(item => item.Id == channelId, cancellationToken);
        if (channel == null) {
            channel = new PublicationChannel { Id = Guid.NewGuid().ToString("N"), CreationTime = _clock.Now };
            _db.PublicationChannels.Add(channel);
        }

        channel.Name = request.Name.Trim();
        channel.Platform = request.Platform;
        channel.Enabled = request.Enabled;
        channel.AppId = request.AppId?.Trim();
        channel.Author = request.Author?.Trim();
        channel.Theme = request.Theme?.Trim();
        channel.PublishUrl = string.IsNullOrWhiteSpace(request.PublishUrl) ? DefaultUrl(request.Platform) : request.PublishUrl.Trim();
        if (!string.IsNullOrWhiteSpace(request.AppSecret)) channel.AppSecret = request.AppSecret;
        channel.LastUpdateTime = _clock.Now;
        await _db.SaveChangesAsync(cancellationToken);
        return PublicationChannelResponse.From(channel);
    }

    public async Task<PostPublicationResponse?> PrepareAsync(string postId, PreparePublicationRequest request, CancellationToken cancellationToken) {
        var post = await _db.Posts.AsNoTracking().FirstOrDefaultAsync(item => item.Id == postId, cancellationToken);
        var channel = await _db.PublicationChannels.AsNoTracking().FirstOrDefaultAsync(item => item.Id == request.ChannelId, cancellationToken);
        if (post == null || channel is not { Enabled: true }) return null;

        var publication = new PostPublication {
            Id = Guid.NewGuid().ToString("N"),
            PostId = post.Id,
            ChannelId = channel.Id,
            Platform = channel.Platform,
            Status = PublicationStatus.Prepared,
            Title = string.IsNullOrWhiteSpace(request.Title) ? post.Title : request.Title.Trim(),
            CoverUrl = request.CoverUrl,
            RenderedContent = Render(post, channel),
            CreationTime = _clock.Now,
            LastUpdateTime = _clock.Now
        };
        _db.PostPublications.Add(publication);
        await _db.SaveChangesAsync(cancellationToken);
        return PostPublicationResponse.From(publication);
    }

    /// <summary>投递快照。公众号只创建草稿，不会群发。</summary>
    public async Task<PostPublicationResponse?> PublishAsync(string publicationId, CancellationToken cancellationToken) {
        var publication = await _db.PostPublications.FirstOrDefaultAsync(item => item.Id == publicationId, cancellationToken);
        if (publication == null) return null;
        var channel = await _db.PublicationChannels.FirstOrDefaultAsync(item => item.Id == publication.ChannelId, cancellationToken);
        if (channel is not { Enabled: true }) return await FailAsync(publication, "发布渠道不存在或已停用", cancellationToken);

        publication.Status = PublicationStatus.Publishing;
        publication.ErrorMessage = null;
        publication.LastUpdateTime = _clock.Now;
        await _db.SaveChangesAsync(cancellationToken);

        try {
            switch (channel.Platform) {
                case PublicationPlatform.WechatOfficialAccount:
                    publication.ExternalId = await PublishWechatDraftAsync(publication, channel, cancellationToken);
                    publication.Status = PublicationStatus.Published;
                    publication.ExternalUrl = "https://mp.weixin.qq.com/";
                    publication.PublishedTime = _clock.Now;
                    break;
                case PublicationPlatform.Zhihu:
                case PublicationPlatform.Juejin:
                    publication.Status = PublicationStatus.Prepared;
                    publication.ExternalUrl = channel.PublishUrl ?? DefaultUrl(channel.Platform);
                    publication.ErrorMessage = "该平台没有配置可用的创作者发布 API，已生成可复制的内容快照。";
                    break;
                default:
                    return await FailAsync(publication, "该渠道尚未实现自动发布适配器", cancellationToken);
            }
        }
        catch (Exception exception) {
            _logger.LogError(exception, "发布文章 {PostId} 到渠道 {ChannelId} 时失败", publication.PostId, channel.Id);
            return await FailAsync(publication, exception.Message, cancellationToken);
        }

        publication.LastUpdateTime = _clock.Now;
        await _db.SaveChangesAsync(cancellationToken);
        return PostPublicationResponse.From(publication);
    }

    private async Task<PostPublicationResponse> FailAsync(PostPublication publication, string message, CancellationToken cancellationToken) {
        publication.Status = PublicationStatus.Failed;
        publication.ErrorMessage = message;
        publication.LastUpdateTime = _clock.Now;
        await _db.SaveChangesAsync(cancellationToken);
        return PostPublicationResponse.From(publication);
    }

    private static string Render(Post post, PublicationChannel channel) {
        if (channel.Platform is PublicationPlatform.Zhihu or PublicationPlatform.Juejin) {
            return post.Content ?? string.Empty;
        }

        var body = MarkdownRenderer.ToHtml(post.Content);
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
        var token = await GetWechatTokenAsync(client, channel, cancellationToken);
        var content = await ReplaceWechatImagesAsync(client, publication.RenderedContent, token, cancellationToken);
        var thumbMediaId = await UploadWechatMediaAsync(client, publication.CoverUrl, token, "thumb", cancellationToken);
        var payload = new {
            articles = new[] {
                new {
                    title = publication.Title,
                    author = channel.Author ?? string.Empty,
                    content,
                    thumb_media_id = thumbMediaId,
                    need_open_comment = 1,
                    only_fans_can_comment = 0
                }
            }
        };
        using var response = await client.PostAsJsonAsync(
            $"https://api.weixin.qq.com/cgi-bin/draft/add?access_token={Uri.EscapeDataString(token)}",
            payload,
            cancellationToken);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        if (!response.IsSuccessStatusCode || !document.RootElement.TryGetProperty("media_id", out var mediaId)) {
            throw new InvalidOperationException(WechatError(document.RootElement));
        }

        return mediaId.GetString() ?? throw new InvalidOperationException("微信草稿接口未返回 media_id");
    }

    private static async Task<string> GetWechatTokenAsync(HttpClient client, PublicationChannel channel, CancellationToken cancellationToken) {
        var url = $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={Uri.EscapeDataString(channel.AppId!)}&secret={Uri.EscapeDataString(channel.AppSecret!)}";
        using var response = await client.GetAsync(url, cancellationToken);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        if (!response.IsSuccessStatusCode || !document.RootElement.TryGetProperty("access_token", out var token)) {
            throw new InvalidOperationException(WechatError(document.RootElement));
        }

        return token.GetString() ?? throw new InvalidOperationException("微信未返回 access_token");
    }

    private async Task<string> ReplaceWechatImagesAsync(HttpClient client, string html, string token, CancellationToken cancellationToken) {
        var matches = ImageSourceRegex.Matches(html);
        foreach (Match match in matches) {
            var url = match.Groups["url"].Value;
            if (string.IsNullOrWhiteSpace(url)) continue;
            var mediaId = await UploadWechatMediaAsync(client, url, token, "image", cancellationToken);
            html = html.Replace(url, $"https://mmbiz.qpic.cn/{mediaId}", StringComparison.Ordinal);
        }

        return html;
    }

    private static async Task<string> UploadWechatMediaAsync(HttpClient client, string imageUrl, string token, string type, CancellationToken cancellationToken) {
        using var image = await client.GetAsync(imageUrl, cancellationToken);
        image.EnsureSuccessStatusCode();
        await using var bytes = await image.Content.ReadAsStreamAsync(cancellationToken);
        using var form = new MultipartFormDataContent();
        form.Add(new StreamContent(bytes), "media", "cover.jpg");
        using var response = await client.PostAsync(
            $"https://api.weixin.qq.com/cgi-bin/material/add_material?access_token={Uri.EscapeDataString(token)}&type={type}",
            form,
            cancellationToken);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        if (!document.RootElement.TryGetProperty("media_id", out var mediaId)) {
            throw new InvalidOperationException(WechatError(document.RootElement));
        }

        return mediaId.GetString() ?? throw new InvalidOperationException("微信素材接口未返回 media_id");
    }

    private static string WechatError(JsonElement element) =>
        element.TryGetProperty("errmsg", out var message) ? message.GetString() ?? "微信接口错误" : "微信接口错误";

    private static string DefaultUrl(PublicationPlatform platform) => platform switch {
        PublicationPlatform.Zhihu => "https://zhuanlan.zhihu.com/write",
        PublicationPlatform.Juejin => "https://juejin.cn/editor/drafts/new?from=nav",
        _ => "https://mp.weixin.qq.com/"
    };
}
