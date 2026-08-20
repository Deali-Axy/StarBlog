using System.ClientModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Infrastructure.Time;
using StarBlog.Api.Modules.Content.Domain;

namespace StarBlog.Api.Modules.Content;

/// <summary>文章翻译的 OpenAI 兼容服务配置。</summary>
public sealed class TranslationOptions {
    public const string SectionName = "Translation";
    public LlmOptions LLM { get; init; } = new();
}

/// <summary>兼容 OpenAI Chat Completions 的模型连接配置。</summary>
public sealed class LlmOptions {
    public string Endpoint { get; init; } = "https://api.openai.com/v1";
    public string Key { get; init; } = string.Empty;
    public string Model { get; init; } = "gpt-4o-mini";
}

/// <summary>
/// 可选翻译能力。未配置密钥时服务仍可启动，调用时返回明确错误。
/// </summary>
public sealed class TranslationOperations {
    private readonly StarBlogDbContext _db;
    private readonly IClock _clock;
    private readonly IChatClient? _chatClient;

    public TranslationOperations(StarBlogDbContext db, IClock clock, IOptions<TranslationOptions> options) {
        _db = db;
        _clock = clock;
        var config = options.Value;
        if (!string.IsNullOrWhiteSpace(config.LLM.Key)) {
            _chatClient = new OpenAIClient(new ApiKeyCredential(config.LLM.Key), new OpenAIClientOptions {
                Endpoint = new Uri(config.LLM.Endpoint)
            }).GetChatClient(config.LLM.Model).AsIChatClient();
        }
    }

    /// <summary>翻译文章为指定语言。已存在的译本会被更新。</summary>
    public async Task<(PostTranslationResponse? Translation, string? Error)> TranslateAsync(string postId, string language, CancellationToken cancellationToken) {
        if (_chatClient == null) return (null, "未配置翻译服务密钥");
        var post = await _db.Posts.FirstOrDefaultAsync(item => item.Id == postId, cancellationToken);
        if (post == null) return (null, null);

        var title = await TranslateTextAsync(post.Title, language, "title", cancellationToken);
        var summary = string.IsNullOrWhiteSpace(post.Summary) ? null : await TranslateTextAsync(post.Summary, language, "summary", cancellationToken);
        var content = string.IsNullOrWhiteSpace(post.Content) ? null : await TranslateTextAsync(post.Content, language, "markdown", cancellationToken);
        var translation = await _db.PostTranslations.FirstOrDefaultAsync(item => item.PostId == postId && item.Language == language, cancellationToken);
        if (translation == null) {
            translation = new PostTranslation {
                Id = Guid.NewGuid().ToString("N"),
                PostId = postId,
                Language = language,
                CreationTime = _clock.Now
            };
            _db.PostTranslations.Add(translation);
        }

        translation.Title = title;
        translation.Summary = summary;
        translation.Content = content;
        translation.LastUpdateTime = _clock.Now;
        await _db.SaveChangesAsync(cancellationToken);
        return (new PostTranslationResponse {
            Id = translation.Id,
            PostId = translation.PostId,
            Language = translation.Language,
            Title = translation.Title,
            Summary = translation.Summary,
            Content = translation.Content
        }, null);
    }

    private async Task<string> TranslateTextAsync(string text, string language, string kind, CancellationToken cancellationToken) {
        var response = await _chatClient!.GetResponseAsync(
            $"Translate the following {kind} into {language}. Keep markdown syntax if present.\n\n{text}",
            cancellationToken: cancellationToken);
        return response.Text;
    }
}
