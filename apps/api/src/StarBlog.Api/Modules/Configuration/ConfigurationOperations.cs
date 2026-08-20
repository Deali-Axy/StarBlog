using Microsoft.EntityFrameworkCore;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Modules.Configuration.Domain;

namespace StarBlog.Api.Modules.Configuration;

/// <summary>
/// 运行时配置读写。缺失项可回退到 StarBlog:Initial:{key}。
/// </summary>
public sealed class ConfigurationOperations : ISiteConfiguration {
    private readonly StarBlogDbContext _db;
    private readonly IConfiguration _configuration;

    public ConfigurationOperations(StarBlogDbContext db, IConfiguration configuration) {
        _db = db;
        _configuration = configuration;
    }

    /// <summary>返回全部配置项。</summary>
    public async Task<List<ConfigItemResponse>> GetAllAsync(CancellationToken cancellationToken) {
        var items = await _db.ConfigItems.AsNoTracking().OrderBy(item => item.Key).ToListAsync(cancellationToken);
        return items.Select(ToResponse).ToList();
    }

    /// <summary>按 key 读取；数据库没有时尝试从初始化配置节写入。</summary>
    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default) {
        var item = await _db.ConfigItems.FirstOrDefaultAsync(entry => entry.Key == key, cancellationToken);
        if (item != null) return item.Value;

        var section = _configuration.GetSection($"StarBlog:Initial:{key}");
        if (!section.Exists() || section.Value == null) return null;

        await SetAsync(key, section.Value, "Initial", cancellationToken);
        return section.Value;
    }

    /// <summary>按 key 获取完整配置项。</summary>
    public async Task<ConfigItemResponse?> GetItemAsync(string key, CancellationToken cancellationToken) {
        var value = await GetAsync(key, cancellationToken);
        if (value == null) return null;
        var item = await _db.ConfigItems.AsNoTracking().FirstAsync(entry => entry.Key == key, cancellationToken);
        return ToResponse(item);
    }

    /// <summary>创建或更新配置。调用方负责 SaveChanges，以便与其他写入共享事务。</summary>
    public async Task SetAsync(string key, string value, string? description = null, CancellationToken cancellationToken = default) {
        var item = await _db.ConfigItems.FirstOrDefaultAsync(entry => entry.Key == key, cancellationToken);
        if (item == null) {
            _db.ConfigItems.Add(new ConfigItem { Key = key, Value = value, Description = description });
            return;
        }

        item.Value = value;
        if (description != null) item.Description = description;
    }

    /// <summary>管理端保存配置并立即提交。</summary>
    public async Task<ConfigItemResponse> UpsertAsync(ConfigItemUpsertRequest request, CancellationToken cancellationToken) {
        await SetAsync(request.Key, request.Value, request.Description, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetItemAsync(request.Key, cancellationToken))!;
    }

    /// <summary>按 key 删除配置。</summary>
    public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken) {
        var item = await _db.ConfigItems.FirstOrDefaultAsync(entry => entry.Key == key, cancellationToken);
        if (item == null) return false;
        _db.ConfigItems.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ConfigItemResponse ToResponse(ConfigItem item) => new() {
        Id = item.Id,
        Key = item.Key,
        Value = item.Value,
        Description = item.Description
    };
}
