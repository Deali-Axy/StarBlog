using System.Text;
using DataProc.Utilities;
using FluentResults;
using FreeSql;
using Microsoft.Extensions.Logging;
using StarBlog.Data.Models;

namespace DataProc.Services;

public class SummaryGenerator(
    ILogger<SlugGenerator> logger,
    IBaseRepository<Post> postRepo,
    LLM llm
) : IService {
    public async Task<Result> Run() {
        var total = await postRepo.Select.CountAsync();
        var posts = await postRepo.Where(e => string.IsNullOrWhiteSpace(e.Summary)).ToListAsync();

        Console.WriteLine($"没有 summary 的 posts: {posts.Count}，总 posts: {total}");

        foreach (var post in posts[..1]) {
            if (string.IsNullOrWhiteSpace(post.Content)) {
                continue;
            }

            var prompt = PromptBuilder
                .Create(PromptTemplates.ArticleDescriptionTechnical)
                .AddParameter("title", post.Title)
                .AddParameter("content", post.Content)
                .Build();


            var textStreamAsync = llm.GenerateTextStreamAsync(prompt);
            var description = new StringBuilder();

            Console.WriteLine("生成文章简介：");
            await foreach (var update in textStreamAsync) {
                description.Append(update.Text);

                Console.Write(update.Text);
            }
        }

        return Result.Ok();
    }
}