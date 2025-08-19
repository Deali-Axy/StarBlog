using FluentResults;
using FreeSql;
using Microsoft.Extensions.Logging;
using StarBlog.Data.Models;

namespace DataProc.Services;

/// <summary>
/// 数据探索脚本
/// </summary>
public class DataInsightScript(
    ILogger<DataInsightScript> logger,
    IBaseRepository<Post> postRepo
) : IService {
    public async Task<Result> Run() {
        var total = await postRepo.Select.CountAsync();
        var posts = await postRepo.Where(e =>
            e.Summary == e.Title ||
            e.Content.StartsWith(e.Summary)
        ).ToListAsync();

        logger.LogInformation("符合条件的文章数量: {Count}, 总数: {Total}", posts.Count, total);

        return Result.Ok();
    }
}