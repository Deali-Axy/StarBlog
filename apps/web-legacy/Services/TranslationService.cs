using System.ClientModel;
using System.Text;
using System.Text.RegularExpressions;
using FreeSql;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using StarBlog.Data.Models;

namespace StarBlog.Web.Services;

/// <summary>
/// 文章翻译配置
/// </summary>
public class TranslationConfig {
    public const string SectionName = "Translation";

    public LLMConfig LLM { get; set; } = new();
    public string TargetLanguage { get; set; } = "en";
    public int MaxContentLength { get; set; } = 6000;
    public int MaxRetries { get; set; } = 3;
}

public class LLMConfig {
    public string Endpoint { get; set; } = "https://api.openai.com/v1";
    public string Key { get; set; } = "";
    public string Model { get; set; } = "gpt-4o-mini";
}

/// <summary>
/// 文章翻译服务
/// </summary>
public class TranslationService {
    private readonly ILogger<TranslationService> _logger;
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<PostTranslation> _translationRepo;
    private readonly TranslationConfig _config;
    private readonly IChatClient _chatClient;

    public TranslationService(
        ILogger<TranslationService> logger,
        IBaseRepository<Post> postRepo,
        IBaseRepository<PostTranslation> translationRepo,
        IOptions<TranslationConfig> config) {
        _logger = logger;
        _postRepo = postRepo;
        _translationRepo = translationRepo;
        _config = config.Value;

        // 初始化 LLM 客户端（OpenAI 兼容接口）
        if (!string.IsNullOrEmpty(_config.LLM.Key)) {
            _chatClient = new OpenAIClient(
                new ApiKeyCredential(_config.LLM.Key),
                new OpenAIClientOptions {
                    Endpoint = new Uri(_config.LLM.Endpoint)
                }
            ).GetChatClient(_config.LLM.Model).AsIChatClient();
        }
    }

    /// <summary>
    /// 获取文章的翻译
    /// </summary>
    public async Task<PostTranslation?> GetTranslation(string postId, string language) {
        return await _translationRepo
            .Where(t => t.PostId == postId && t.Language == language)
            .FirstAsync();
    }

    /// <summary>
    /// 获取文章的可用翻译语言列表
    /// </summary>
    public async Task<List<string>> GetAvailableLanguages(string postId) {
        return await _translationRepo
            .Where(t => t.PostId == postId)
            .ToListAsync(t => t.Language);
    }

    /// <summary>
    /// 翻译单篇文章
    /// </summary>
    public async Task<PostTranslation> TranslatePostAsync(string postId, string language) {
        EnsureLLMClient();

        var post = await _postRepo.Where(p => p.Id == postId).FirstAsync();
        if (post == null) throw new Exception($"文章 {postId} 不存在");

        // 检查是否已有翻译，有则更新
        var existing = await _translationRepo
            .Where(t => t.PostId == postId && t.Language == language)
            .FirstAsync();

        _logger.LogInformation("开始翻译文章: {Title} -> {Lang}", post.Title, language);

        // 1. 翻译标题
        var title = await TranslateText(post.Title, language, "title");
        _logger.LogInformation("  标题翻译完成: {Title}", title);

        // 2. 翻译摘要
        string? summary = null;
        if (!string.IsNullOrWhiteSpace(post.Summary)) {
            summary = await TranslateText(post.Summary, language, "summary");
            _logger.LogInformation("  摘要翻译完成");
        }

        // 3. 翻译 Markdown 内容
        string? content = null;
        if (!string.IsNullOrWhiteSpace(post.Content)) {
            content = await TranslateMarkdown(post.Content, language);
            _logger.LogInformation("  内容翻译完成");
        }

        // 4. 保存或更新翻译
        if (existing != null) {
            existing.Title = title;
            existing.Summary = summary;
            existing.Content = content;
            existing.LastUpdateTime = DateTime.Now;
            await _translationRepo.UpdateAsync(existing);
            _logger.LogInformation("翻译已更新: {Title}", post.Title);
            return existing;
        }
        else {
            var translation = new PostTranslation {
                Id = Guid.NewGuid().ToString(),
                PostId = postId,
                Language = language,
                Title = title,
                Summary = summary,
                Content = content,
                CreationTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            };
            await _translationRepo.InsertAsync(translation);
            _logger.LogInformation("翻译已保存: {Title}", post.Title);
            return translation;
        }
    }

    /// <summary>
    /// 删除翻译
    /// </summary>
    public async Task<int> DeleteTranslation(string translationId) {
        return await _translationRepo.Where(t => t.Id == translationId).ToDelete().ExecuteAffrowsAsync();
    }

    private async Task<string> TranslateText(string text, string language, string type) {
        var prompt = type switch {
            "title" => $"""
                Translate the following Chinese text to {language}.
                Return ONLY the translated text, nothing else.
                Preserve any technical terms, brand names, or proper nouns.

                Text: {text}
                """,
            "summary" => $"""
                Translate the following Chinese text to {language}.
                Return ONLY the translated text, nothing else.
                Preserve any technical terms, brand names, or proper nouns.

                Text: {text}
                """,
            _ => text
        };

        return await GenerateWithRetry(prompt);
    }

    private async Task<string> TranslateMarkdown(string markdown, string language) {
        var sections = SplitMarkdownBySections(markdown);
        var translatedSections = new List<string>();

        foreach (var section in sections) {
            if (section.Length > _config.MaxContentLength) {
                // 大段落按段落拆分
                var paragraphs = section.Split("\n\n");
                var translatedParagraphs = new List<string>();
                foreach (var para in paragraphs) {
                    if (string.IsNullOrWhiteSpace(para)) {
                        translatedParagraphs.Add(para);
                        continue;
                    }

                    translatedParagraphs.Add(await TranslateSingleSection(para, language));
                }

                translatedSections.Add(string.Join("\n\n", translatedParagraphs));
            }
            else {
                translatedSections.Add(await TranslateSingleSection(section, language));
            }
        }

        return string.Join("\n", translatedSections);
    }

    private async Task<string> TranslateSingleSection(string section, string language) {
        var prompt = $"""
            You are a professional translator specializing in technical content.
            Translate the following Markdown content from Chinese to {language}.

            Rules:
            1. Preserve ALL Markdown formatting (headings, lists, links, bold, italic, tables)
            2. Do NOT translate content inside code blocks (```...```)
            3. Do NOT translate inline code (`...`)
            4. Do NOT translate URLs or image paths
            5. Preserve all HTML tags if any
            6. Keep the translation natural and fluent
            7. For technical terms, keep the original Chinese in parentheses if helpful

            Return ONLY the translated Markdown content.

            Content:
            {section}
            """;

        return await GenerateWithRetry(prompt);
    }

    private async Task<string> GenerateWithRetry(string prompt) {
        for (int attempt = 1; attempt <= _config.MaxRetries; attempt++) {
            try {
                var sb = new StringBuilder();
                await foreach (var update in _chatClient.GetStreamingResponseAsync(prompt)) {
                    if (!string.IsNullOrEmpty(update.Text)) {
                        sb.Append(update.Text);
                    }
                }

                return sb.ToString().Trim();
            }
            catch (Exception ex) when (attempt < _config.MaxRetries) {
                _logger.LogWarning(ex, "翻译请求第 {Attempt} 次失败，{Max} 次后重试", attempt, _config.MaxRetries);
                await Task.Delay(1000 * (int)Math.Pow(2, attempt));
            }
        }

        throw new Exception($"翻译失败，已重试 {_config.MaxRetries} 次");
    }

    private void EnsureLLMClient() {
        if (_chatClient == null) {
            throw new Exception("翻译服务未配置 LLM API Key，请在 appsettings.json 的 Translation:LLM:Key 中设置");
        }
    }

    /// <summary>
    /// 按 Markdown 标题分段（H1/H2/H3）
    /// </summary>
    private static List<string> SplitMarkdownBySections(string markdown) {
        var lines = markdown.Split('\n');
        var sections = new List<string>();
        var current = new List<string>();

        foreach (var line in lines) {
            if (Regex.IsMatch(line, @"^#{1,3}\s")) {
                if (current.Count > 0) {
                    sections.Add(string.Join("\n", current));
                    current.Clear();
                }
            }

            current.Add(line);
        }

        if (current.Count > 0) {
            sections.Add(string.Join("\n", current));
        }

        return sections;
    }
}
