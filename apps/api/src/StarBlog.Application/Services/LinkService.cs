using FreeSql;
using StarBlog.Data.Models;

namespace StarBlog.Application.Services;

/// <summary>
/// 友情链接
/// </summary>
public class LinkService {
    private IBaseRepository<Link> _repo;

    public LinkService(IBaseRepository<Link> repo) {
        _repo = repo;
    }

    /// <summary>
    /// 获取全部友情链接
    /// </summary>
    /// <param name="onlyVisible">只获取显示的链接</param>
    public async Task<List<Link>> GetAll(bool onlyVisible = true) {
        return onlyVisible
            ? await _repo.Where(a => a.Visible).ToListAsync()
            : await _repo.Select.ToListAsync();
    }

    public async Task<Link?> GetById(int id) {
        return await _repo.Where(a => a.Id == id).FirstAsync();
    }

    public async Task<Link?> GetByName(string name) {
        return await _repo.Where(a => a.Name == name).FirstAsync();
    }

    /// <summary>
    /// 查询 id 是否存在
    /// </summary>
    public async Task<bool> HasId(int id) {
        return await _repo.Where(a => a.Id == id).AnyAsync();
    }

    public async Task<Link> AddOrUpdate(Link item) {
        return await _repo.InsertOrUpdateAsync(item);
    }

    public async Task<Link?> SetVisibility(int id, bool visible) {
        var item = await GetById(id);
        if (item == null) return null;
        item.Visible = visible;
        await _repo.UpdateAsync(item);
        return item;
    }

    public async Task<int> DeleteById(int id) {
        return await _repo.DeleteAsync(a => a.Id == id);
    }
}