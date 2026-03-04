using FluentResults;
using FreeSql;
using Microsoft.Extensions.Logging;
using StarBlog.Data.Models;

namespace DataProc.Services;

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
        var category = await categoryRepo.Where(e => e.Name == "开箱评测").FirstAsync();
        var total = await postRepo.Select.CountAsync();
        // 使用 FreeSql 的 AsTreeCte 递归获取所有子分类 ID（包括当前分类）
        var targetCategoryIds = await categoryRepo
            .Where(a => a.Id == category.Id)
            .AsTreeCte()
            .ToListAsync(a => a.Id);
        var posts = await postRepo.Where(e =>
            targetCategoryIds.Contains(e.CategoryId)
        ).ToListAsync();

        logger.LogInformation("符合条件的文章数量: {Count}, 总数: {Total}", posts.Count, total);

        return Result.Ok();
    }
}