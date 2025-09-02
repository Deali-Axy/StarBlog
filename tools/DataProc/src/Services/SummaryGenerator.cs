using System.Text;
using DataProc.Entities;
using DataProc.Utilities;
using FluentResults;
using FreeSql;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarBlog.Data.Models;

namespace DataProc.Services;

public class SummaryGenerator(
    ILogger<SummaryGenerator> logger,
    IBaseRepository<Post> postRepo,
    LLM llm,
    IOptions<SummaryGeneratorSettings> options
) : IService {
    private readonly SummaryGeneratorSettings _settings = options.Value;


    public async Task<Result> Run() {
            var total = await postRepo.Select.CountAsync();
        var posts = await postRepo.Where(e =>
            string.IsNullOrWhiteSpace(e.Summary) ||
            e.Summary == e.Title
        ).ToListAsync();

        logger.LogInformation("开始生成文章摘要 - 待处理: {Count}, 总数: {Total}", posts.Count, total);

        var successCount = 0;
        var failureCount = 0;

        foreach (var post in posts) {
            try {
                if (string.IsNullOrWhiteSpace(post.Content)) {
                    logger.LogWarning("文章 [{title}] 内容为空，跳过", post.Title);
                    continue;
                }

                var result = await GenerateSummaryWithRetry(post);
                if (result.IsSuccess) {
                    successCount++;
                    logger.LogInformation("文章 [{title}] 摘要生成成功", post.Title);
                }
                else {
                    failureCount++;
                    logger.LogError("文章 [{title}] 摘要生成失败: {Error}", post.Title,
                        result.Errors.FirstOrDefault()?.Message);
                }

                // 添加延迟以避免速率限制
                await Task.Delay(_settings.DelayBetweenRequests);
            }
            catch (Exception ex) {
                failureCount++;
                logger.LogError(ex, "处理文章 [{title}] 时发生未预期错误", post.Title);
            }
        }

        logger.LogInformation("摘要生成完成 - 成功: {Success}, 失败: {Failure}", successCount, failureCount);
        return Result.Ok();
    }

    private async Task<Result> GenerateSummaryWithRetry(Post post) {
        for (int attempt = 1; attempt <= _settings.MaxRetries; attempt++) {
            try {
                // 截断过长的内容
                var content = post.Content.Length > _settings.MaxContentLength
                    ? post.Content.Substring(0, _settings.MaxContentLength) + "..."
                    : post.Content;

                var prompt = PromptBuilder
                    .Create(PromptTemplates.ArticleDescriptionTechnical)
                    .AddParameter("title", post.Title)
                    .AddParameter("content", content)
                    .Build();

                var summary = await GenerateSummaryStream(prompt, post.Title);

                if (!string.IsNullOrWhiteSpace(summary)) {
                    post.Summary = summary.Trim();
                    await postRepo.UpdateAsync(post);
                    return Result.Ok();
                }

                return Result.Fail("生成的摘要为空");
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

    private async Task<string> GenerateSummaryStream(string prompt, string title) {
        var description = new StringBuilder();

        logger.LogInformation("开始为文章 [{Title}] 生成摘要", title);

        try {
            var textStreamAsync = llm.GenerateTextStreamAsync(prompt);

            await foreach (var update in textStreamAsync) {
                if (!string.IsNullOrEmpty(update.Text)) {
                    description.Append(update.Text);
                    Console.Write(update.Text);
                }
            }

            Console.WriteLine(); // 换行
            return description.ToString();
        }
        catch (Exception ex) {
            logger.LogError(ex, "流式生成摘要时发生错误");
            throw;
        }
    }
}