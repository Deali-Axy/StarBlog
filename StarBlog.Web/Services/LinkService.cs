using FreeSql;
using StarBlog.Data.Models;

namespace StarBlog.Web.Services;

public class LinkService {
    private IBaseRepository<Link> _repo;

    public LinkService(IBaseRepository<Link> repo) {
        _repo = repo;
    }

    /// <summary>
    /// 获取全部友情链接
    /// </summary>
    /// <param name="onlyVisible">只获取显示的链接</param>
    /// <returns></returns>
    public List<Link> GetAll(bool onlyVisible = true) {
        return onlyVisible
            ? _repo.Where(a => a.Visible).ToList()
            : _repo.Select.ToList();
    }

    public Link? GetById(int id) {
        return _repo.Where(a => a.Id == id).First();
    }

    public Link? GetByName(string name) {
        return _repo.Where(a => a.Name == name).First();
    }

    /// <summary>
    /// 查询 id 是否存在
    /// </summary>
    public bool HasId(int id) {
        return _repo.Where(a => a.Id == id).Any();
    }

    public Link AddOrUpdate(Link item) {
        return _repo.InsertOrUpdate(item);
    }

    public Link? SetVisibility(int id, bool visible) {
        var item = GetById(id);
        if (item == null) return null;
        item.Visible = visible;
        _repo.Update(item);
        return GetById(id);
    }

    public int DeleteById(int id) {
        return _repo.Delete(a => a.Id == id);
    }
}