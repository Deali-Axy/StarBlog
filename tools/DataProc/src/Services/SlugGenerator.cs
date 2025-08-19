using DataProc.Utilities;
using FluentResults;
using FreeSql;
using Microsoft.Extensions.Logging;
using StarBlog.Data.Models;

namespace DataProc.Services;

public class SlugGenerator(
    ILogger<SlugGenerator> logger,
    IBaseRepository<Post> postRepo
) : IService {
    public async Task<Result> Run() {
        var total = await postRepo.Select.CountAsync();
        var posts = await postRepo.Where(e => string.IsNullOrWhiteSpace(e.Slug)).ToListAsync();

        foreach (var post in posts) {
            var prompt = PromptBuilder
                .Create(PromptTemplates.UrlSlugGeneration)
                .AddParameter("title", post.Title)
                .Build();
        }

        Console.WriteLine($"没有 slug 的 posts: {posts.Count}，总 posts: {total}");

        return Result.Ok();
    }
}