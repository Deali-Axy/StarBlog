using FluentResults;
using FreeSql;
using Microsoft.Extensions.Logging;
using StarBlog.Data.Models;

namespace DataProc.Services;

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Utilities;

/// <summary>
/// 数据探索脚本，
/// 可以在此脚本里查询任意数据
/// </summary>
public class DataInsightScript(
    ILogger<DataInsightScript> logger,
    IBaseRepository<Post> postRepo,
    IBaseRepository<Category> categoryRepo
) : IService {
    public async Task<Result> Run() {
        const string categoryName = "开箱评测";
        var category = await categoryRepo.Where(e => e.Name == categoryName).FirstAsync();
        var total = await postRepo.Select.CountAsync();
        // 使用 FreeSql 的 AsTreeCte 递归获取所有子分类 ID（包括当前分类）
        var targetCategoryIds = await categoryRepo
            .Where(a => a.Id == category.Id)
            .AsTreeCte()
            .ToListAsync(a => a.Id);
        var posts = await postRepo.Where(e =>
                targetCategoryIds.Contains(e.CategoryId)
            )
            .Include(e => e.Category)
            .ToListAsync();

        logger.LogInformation(
            "筛选分类: {Category}，符合条件的文章数量: {Count}, 总数: {Total}",
            categoryName, posts.Count, total
        );

        var metadatas = posts.Select(e =>
            new PostMetadata(
                e.Id, e.Title, e.Summary ?? "",
                e.CreationTime, e.Category?.Name ?? "开箱评测",
                e.GetSections("前言", "小结")
            )
        );

        var prompt = PromptBuilder.Create(PromptTemplates.PostsRetrospectiveArchitectUnboxingEdition)
            .AddParameter("time_range", "2023-2026年")
            .AddParameter("posts", Json.Dumps(metadatas))
            .Build();

        Console.WriteLine(prompt);

        return Result.Ok();
    }
}

record PostMetadata(
    string Id,
    string Title,
    string Summary,
    DateTime CreatedAt,
    string Category,
    Dictionary<string, string> Sections
);

// dotnet10 新的扩展成员（Extension Members）语法
public static class MyExtensions {
    extension(Post post) {
        public string SimpleExtProp => "hello";

        public string GetSection(string title) {
            return ExtractSectionRegex(post.Content, title);
        }

        public Dictionary<string, string> GetSections(params string[] titles) {
            return titles.ToDictionary(title => title, post.GetSection);
        }
    }

    private static string ExtractSectionRegex(string content, string title) {
        // 匹配目标标题开始，直到下一个标题或文档末尾
        string pattern = $@"##\s*{title}\s*([\s\S]*?)(?=\n##|$)";
        var match = Regex.Match(content, pattern);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }
}