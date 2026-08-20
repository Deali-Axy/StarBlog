using System.Text.Json;
using CodeLab.Share.Contrib.StopWords;

namespace StarBlog.Application.Services;

public class TempFilterService {
    private readonly ILogger<TempFilterService> _logger;
    private readonly StopWordsToolkit _toolkit;

    public StopWordsToolkit Toolkit => _toolkit;

    public TempFilterService(ILogger<TempFilterService> logger) {
        _logger = logger;
        IEnumerable<Word>? words = null;
        const string wordsPath = "words.json";
        if (File.Exists(wordsPath)) {
            words = JsonSerializer.Deserialize<IEnumerable<Word>>(File.ReadAllText(wordsPath));
        }
        else {
            _logger.LogWarning("未找到 words.json 文件，评论敏感词检测服务可能无法正常工作！");
        }

        _toolkit = words != null
            ? new StopWordsToolkit(words.Select(a => a.Value))
            : new StopWordsToolkit(Array.Empty<string>());
    }

    public bool CheckBadWord(string word) {
        return _toolkit.CheckBadWord(word);
    }
}