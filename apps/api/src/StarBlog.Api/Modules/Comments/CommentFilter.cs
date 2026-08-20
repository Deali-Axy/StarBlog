using System.Text.Json;

namespace StarBlog.Api.Modules.Comments;

/// <summary>
/// 简单敏感词检测。未找到 words.json 时不拦截。
/// </summary>
public sealed class CommentFilter {
    private readonly HashSet<string> _words;

    public CommentFilter(IWebHostEnvironment environment, ILogger<CommentFilter> logger) {
        var path = Path.Combine(environment.ContentRootPath, "words.json");
        if (!File.Exists(path)) {
            logger.LogWarning("未找到 words.json，评论敏感词检测未启用");
            _words = [];
            return;
        }

        using var stream = File.OpenRead(path);
        var items = JsonSerializer.Deserialize<List<WordItem>>(stream) ?? [];
        _words = items
            .Select(item => item.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>内容是否包含敏感词。</summary>
    public bool ContainsBadWord(string content) =>
        _words.Count > 0 && _words.Any(word => content.Contains(word, StringComparison.OrdinalIgnoreCase));

    private sealed class WordItem {
        public string Value { get; set; } = string.Empty;
    }
}
