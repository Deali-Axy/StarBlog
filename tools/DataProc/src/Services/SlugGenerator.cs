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

        if (posts.Count == 0) {
            Console.WriteLine("✅ 所有文章都已有 Slug，无需处理");
            return Result.Ok();
        }

        var successCount = 0;
        var failureCount = 0;
        var skippedCount = 0;
        var startTime = DateTime.Now;

        Console.WriteLine($"\n🚀 开始处理 {posts.Count} 篇文章...\n");

        for (int i = 0; i < posts.Count; i++) {
            var post = posts[i];
            var currentIndex = i + 1;

            try {
                // 显示当前处理的文章信息
                Console.Write($"[{currentIndex:D3}/{posts.Count:D3}] ");

                if (string.IsNullOrWhiteSpace(post.Title)) {
                    Console.WriteLine($"⚠️  跳过 - 标题为空");
                    logger.LogWarning("文章 [{id}] 标题为空，跳过", post.Id);
                    skippedCount++;
                    continue;
                }
                
                if (string.IsNullOrWhiteSpace(post.Summary)) {
                    Console.WriteLine($"⚠️  跳过 - 简介为空");
                    logger.LogWarning("文章 [{title}] 简介为空，跳过", post.Title);
                    skippedCount++;
                    continue;
                }

                // 显示正在处理的文章标题（截断长标题）
                var displayTitle = post.Title.Length > 40 ? post.Title.Substring(0, 37) + "..." : post.Title;
                Console.Write($"处理: {displayTitle}");

                var result = await GenerateSlugWithRetry(post);
                if (result.IsSuccess) {
                    successCount++;
                    Console.WriteLine($" ✅ {post.Slug}");
                    logger.LogInformation("文章 [{title}] Slug 生成成功: {Slug}", post.Title, post.Slug);
                }
                else {
                    failureCount++;
                    Console.WriteLine($" ❌ 失败");
                    logger.LogError("文章 [{title}] Slug 生成失败: {Error}", post.Title, result.Errors.FirstOrDefault()?.Message);
                }

                // 显示进度统计
                DisplayProgress(currentIndex, posts.Count, successCount, failureCount, skippedCount, startTime);

                // 添加延迟以避免速率限制
                if (currentIndex < posts.Count) {
                    await Task.Delay(_settings.DelayBetweenRequests);
                }
            }
            catch (Exception ex) {
                failureCount++;
                Console.WriteLine($" ❌ 异常");
                logger.LogError(ex, "处理文章 [{title}] 时发生未预期错误", post.Title);

                // 显示进度统计
                DisplayProgress(currentIndex, posts.Count, successCount, failureCount, skippedCount, startTime);
            }
        }

        // 显示最终结果
        var endTime = DateTime.Now;
        var totalTime = endTime - startTime;

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("📊 处理完成统计");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine($"✅ 成功: {successCount} 篇");
        Console.WriteLine($"❌ 失败: {failureCount} 篇");
        Console.WriteLine($"⚠️  跳过: {skippedCount} 篇");
        Console.WriteLine($"📝 总计: {posts.Count} 篇");
        Console.WriteLine($"⏱️  耗时: {totalTime:hh\\:mm\\:ss}");
        Console.WriteLine($"⚡ 平均: {(totalTime.TotalSeconds / posts.Count):F1} 秒/篇");
        Console.WriteLine(new string('=', 60));

        logger.LogInformation("Slug 生成完成 - 成功: {Success}, 失败: {Failure}, 跳过: {Skipped}, 耗时: {Duration}",
            successCount, failureCount, skippedCount, totalTime);

        return Result.Ok();
    }

    private void DisplayProgress(int current, int total, int success, int failure, int skipped, DateTime startTime) {
        var elapsed = DateTime.Now - startTime;
        var percentage = (double)current / total * 100;
        var remaining = total - current;

        // 估算剩余时间
        var avgTimePerItem = elapsed.TotalSeconds / current;
        var estimatedRemaining = TimeSpan.FromSeconds(avgTimePerItem * remaining);

        // 创建进度条
        var progressBarWidth = 30;
        var filledWidth = (int)(percentage / 100 * progressBarWidth);
        var progressBar = new string('█', filledWidth) + new string('░', progressBarWidth - filledWidth);

        Console.WriteLine($"    📈 进度: [{progressBar}] {percentage:F1}% | " +
                         $"✅{success} ❌{failure} ⚠️{skipped} | " +
                         $"剩余: ~{estimatedRemaining:mm\\:ss}");
        Console.WriteLine();
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

            Console.Write(" 🤖 ");

            await foreach (var update in textStreamAsync) {
                if (!string.IsNullOrEmpty(update.Text)) {
                    slugBuilder.Append(update.Text);
                    Console.Write(update.Text);
                }
            }

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