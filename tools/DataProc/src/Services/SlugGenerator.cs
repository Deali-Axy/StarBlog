using System.Text;
using DataProc.Entities;
using DataProc.Utilities;
using FluentResults;
using FreeSql;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarBlog.Data.Models;

namespace DataProc.Services;

public class SlugGenerator(
    ILogger<SlugGenerator> logger,
    IBaseRepository<Post> postRepo,
    LLM llm,
    IOptions<SlugGeneratorSettings> options
) : IService {
    private readonly SlugGeneratorSettings _settings = options.Value;

    public async Task<Result> Run() {
        var total = await postRepo.Select.CountAsync();
        var posts = await postRepo.Where(e => string.IsNullOrWhiteSpace(e.Slug)).ToListAsync();

        logger.LogInformation("开始生成文章 Slug - 待处理: {Count}, 总数: {Total}", posts.Count, total);

        var successCount = 0;
        var failureCount = 0;

        foreach (var post in posts) {
            try {
                if (string.IsNullOrWhiteSpace(post.Title)) {
                    logger.LogWarning("文章 [{id}] 标题为空，跳过", post.Id);
                    continue;
                }

                var result = await GenerateSlugWithRetry(post);
                if (result.IsSuccess) {
                    successCount++;
                    logger.LogInformation("文章 [{title}] Slug 生成成功: {Slug}", post.Title, post.Slug);
                }
                else {
                    failureCount++;
                    logger.LogError("文章 [{title}] Slug 生成失败: {Error}", post.Title, result.Errors.FirstOrDefault()?.Message);
                }

                // 添加延迟以避免速率限制
                await Task.Delay(_settings.DelayBetweenRequests);
            }
            catch (Exception ex) {
                failureCount++;
                logger.LogError(ex, "处理文章 [{title}] 时发生未预期错误", post.Title);
            }
        }

        logger.LogInformation("Slug 生成完成 - 成功: {Success}, 失败: {Failure}", successCount, failureCount);
        return Result.Ok();
    }

    private async Task<Result> GenerateSlugWithRetry(Post post) {
        for (int attempt = 1; attempt <= _settings.MaxRetries; attempt++) {
            try {
                var prompt = PromptBuilder
                    .Create(PromptTemplates.UrlSlugGeneration)
                    .AddParameter("title", post.Title)
                    .AddParameter("summary", post.Summary ?? "")
                    .Build();

                var slug = await GenerateSlugStream(prompt, post.Title);

                if (!string.IsNullOrWhiteSpace(slug)) {
                    // 清理和验证生成的 slug
                    var cleanSlug = CleanSlug(slug.Trim());

                    if (!string.IsNullOrWhiteSpace(cleanSlug)) {
                        // 检查 slug 是否已存在，如果存在则添加后缀
                        var uniqueSlug = await EnsureUniqueSlug(cleanSlug);
                        post.Slug = uniqueSlug;
                        await postRepo.UpdateAsync(post);
                        return Result.Ok();
                    }
                }

                return Result.Fail("生成的 Slug 为空或无效");
            }
            catch (Exception ex) {
                logger.LogWarning("文章 [{title}] 第 {Attempt} 次尝试失败: {Error}",
                    post.Title, attempt, ex.Message);

                if (attempt == _settings.MaxRetries) {
                    return Result.Fail($"重试 {_settings.MaxRetries} 次后仍然失败: {ex.Message}");
                }

                // 指数退避
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                await Task.Delay(delay);
            }
        }

        return Result.Fail("未知错误");
    }

    private async Task<string> GenerateSlugStream(string prompt, string title) {
        var slugBuilder = new StringBuilder();

        logger.LogInformation("开始为文章 [{Title}] 生成 Slug", title);

        try {
            var textStreamAsync = llm.GenerateTextStreamAsync(prompt);

            await foreach (var update in textStreamAsync) {
                if (!string.IsNullOrEmpty(update.Text)) {
                    slugBuilder.Append(update.Text);
                    Console.Write(update.Text);
                }
            }

            Console.WriteLine(); // 换行
            return slugBuilder.ToString();
        }
        catch (Exception ex) {
            logger.LogError(ex, "流式生成 Slug 时发生错误");
            throw;
        }
    }

    private string CleanSlug(string slug) {
        if (string.IsNullOrWhiteSpace(slug)) return string.Empty;

        // 移除引号和其他不需要的字符
        slug = slug.Trim('"', '\'', ' ', '\n', '\r', '\t');

        // 确保只包含字母、数字和连字符
        var cleanedSlug = new StringBuilder();
        foreach (char c in slug.ToLowerInvariant()) {
            if (char.IsLetterOrDigit(c) || c == '-') {
                cleanedSlug.Append(c);
            }
        }

        var result = cleanedSlug.ToString();

        // 移除连续的连字符
        while (result.Contains("--")) {
            result = result.Replace("--", "-");
        }

        // 移除开头和结尾的连字符
        result = result.Trim('-');

        // 限制长度
        if (result.Length > _settings.MaxSlugLength) {
            result = result.Substring(0, _settings.MaxSlugLength).TrimEnd('-');
        }

        return result;
    }

    private async Task<string> EnsureUniqueSlug(string baseSlug) {
        var slug = baseSlug;
        var counter = 1;

        while (await postRepo.Select.AnyAsync(p => p.Slug == slug)) {
            slug = $"{baseSlug}-{counter}";
            counter++;

            // 防止无限循环
            if (counter > 1000) {
                slug = $"{baseSlug}-{Guid.NewGuid().ToString("N")[..8]}";
                break;
            }
        }

        return slug;
    }
}