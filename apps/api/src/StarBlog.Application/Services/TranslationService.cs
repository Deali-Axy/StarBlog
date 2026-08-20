using System.ClientModel;
using System.Text;
using System.Text.RegularExpressions;
using FreeSql;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using StarBlog.Data.Models;

namespace StarBlog.Application.Services;

/// <summary>文章翻译的 OpenAI 兼容服务配置。</summary>
public sealed class TranslationConfig {
    public const string SectionName = "Translation";
    public LlmConfig LLM { get; init; } = new();
    public int MaxContentLength { get; init; } = 6000;
    public int MaxRetries { get; init; } = 3;
}

/// <summary>兼容 OpenAI Chat Completions 的模型连接配置。</summary>
public sealed class LlmConfig {
    public string Endpoint { get; init; } = "https://api.openai.com/v1";
    public string Key { get; init; } = string.Empty;
    public string Model { get; init; } = "gpt-4o-mini";
}

/// <summary>
/// 迁移自 Razor 项目的文章翻译服务。服务不配置密钥时仍可启动，调用翻译接口时才返回明确错误。
/// </summary>
public sealed class TranslationService {
    private readonly ILogger<TranslationService> _logger;
    private readonly IBaseRepository<Post> _postRepository;
    private readonly IBaseRepository<PostTranslation> _translationRepository;
    private readonly TranslationConfig _config;
    private readonly IChatClient? _chatClient;

    public TranslationService(
        ILogger<TranslationService> logger,
        IBaseRepository<Post> postRepository,
        IBaseRepository<PostTranslation> translationRepository,
        IOptions<TranslationConfig> options) {
        _logger = logger;
        _postRepository = postRepository;
        _translationRepository = translationRepository;
        _config = options.Value;
        if (!string.IsNullOrWhiteSpace(_config.LLM.Key)) {
            _chatClient = new OpenAIClient(new ApiKeyCredential(_config.LLM.Key), new OpenAIClientOptions {
                Endpoint = new Uri(_config.LLM.Endpoint)
            }).GetChatClient(_config.LLM.Model).AsIChatClient();
        }
    }

    public Task<PostTranslation?> GetTranslation(string postId, string language) =>
        _translationRepository.Where(item => item.PostId == postId && item.Language == language).FirstAsync();

    public Task<List<string>> GetAvailableLanguages(string postId) =>
        _translationRepository.Where(item => item.PostId == postId).ToListAsync(item => item.Language);

    public async Task<PostTranslation> TranslatePostAsync(string postId, string language) {
        EnsureClient();
        var post = await _postRepository.Where(item => item.Id == postId).FirstAsync();
        if (post == null) throw new InvalidOperationException($"文章 {postId} 不存在");

        // 已存在的语言版本会被更新，保证重复请求不会产生重复翻译记录。
        var translation = await GetTranslation(postId, language);
        var title = await TranslateText(post.Title, language, "title");
        var summary = string.IsNullOrWhiteSpace(post.Summary) ? null : await TranslateText(post.Summary, language, "summary");
        var content = string.IsNullOrWhiteSpace(post.Content) ? null : await TranslateMarkdown(post.Content, language);

        if (translation != null) {
            translation.Title = title;
            translation.Summary = summary;
            translation.Content = content;
            translation.LastUpdateTime = DateTime.Now;
            await _translationRepository.UpdateAsync(translation);
            return translation;
        }

        translation = new PostTranslation {
            Id = Guid.NewGuid().ToString(),
            PostId = postId,
            Language = language,
            Title = title,
            Summary = summary,
            Content = content,
            CreationTime = DateTime.Now,
            LastUpdateTime = DateTime.Now
        };
        await _translationRepository.InsertAsync(translation);
        return translation;
    }

    public Task<int> DeleteTranslation(string translationId) =>
        _translationRepository.Where(item => item.Id == translationId).ToDelete().ExecuteAffrowsAsync();

    private Task<string> TranslateText(string text, string language, string kind) => GenerateWithRetry($"""
        Translate the following Chinese {kind} to {language}.
        Return only the translation. Preserve proper nouns and technical terms.

        Text: {text}
        """);

    private async Task<string> TranslateMarkdown(string markdown, string language) {
        // 以标题分段，使超长文章保持在模型上下文和单次请求限制内。
        var sections = SplitByHeadings(markdown);
        var translated = new List<string>();
        foreach (var section in sections) {
            if (section.Length <= _config.MaxContentLength) {
                translated.Add(await TranslateMarkdownSection(section, language));
                continue;
            }

            var paragraphs = section.Split("\n\n", StringSplitOptions.None);
            translated.Add(string.Join("\n\n", await Task.WhenAll(paragraphs.Select(paragraph =>
                string.IsNullOrWhiteSpace(paragraph) ? Task.FromResult(paragraph) : TranslateMarkdownSection(paragraph, language)))));
        }
        return string.Join("\n", translated);
    }

    private Task<string> TranslateMarkdownSection(string content, string language) => GenerateWithRetry($"""
        Translate this Markdown from Chinese to {language}.
        Preserve Markdown, HTML, URLs, image paths, inline code, and fenced code blocks.
        Return only translated Markdown.

        Content:
        {content}
        """);

    private async Task<string> GenerateWithRetry(string prompt) {
        var client = _chatClient ?? throw new InvalidOperationException("翻译服务未配置 LLM API Key");
        Exception? lastError = null;
        for (var attempt = 1; attempt <= _config.MaxRetries; attempt++) {
            try {
                var result = new StringBuilder();
                await foreach (var update in client.GetStreamingResponseAsync(prompt)) result.Append(update.Text);
                return result.ToString().Trim();
            }
            catch (Exception exception) when (attempt < _config.MaxRetries) {
                lastError = exception;
                _logger.LogWarning(exception, "翻译请求第 {Attempt} 次失败，将重试", attempt);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
        }
        throw new InvalidOperationException($"翻译失败，已重试 {_config.MaxRetries} 次", lastError);
    }

    private void EnsureClient() {
        if (_chatClient == null) throw new InvalidOperationException("翻译服务未配置 LLM API Key，请设置 Translation:LLM:Key");
    }

    private static List<string> SplitByHeadings(string markdown) {
        var sections = new List<string>();
        var current = new List<string>();
        foreach (var line in markdown.Split('\n')) {
            if (Regex.IsMatch(line, @"^#{1,3}\s") && current.Count > 0) {
                sections.Add(string.Join("\n", current));
                current.Clear();
            }
            current.Add(line);
        }
        if (current.Count > 0) sections.Add(string.Join("\n", current));
        return sections;
    }
}
