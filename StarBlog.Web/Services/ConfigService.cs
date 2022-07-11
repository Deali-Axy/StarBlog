using FreeSql;
using StarBlog.Data.Models;

namespace StarBlog.Web.Services;

public class ConfigService {
    private readonly IBaseRepository<ConfigItem> _repo;

    public ConfigService(IBaseRepository<ConfigItem> repo) {
        _repo = repo;
    }

    public List<ConfigItem> GetAll() {
        return _repo.Select.ToList();
    }

    public ConfigItem? GetById(int id) {
        return _repo.Where(a => a.Id == id).First();
    }

    public ConfigItem? GetByKey(string key) {
        return _repo.Where(a => a.Key == key).First();
    }

    public ConfigItem AddOrUpdate(ConfigItem item) {
        return _repo.InsertOrUpdate(item);
    }

    public int DeleteById(int id) {
        return _repo.Delete(a => a.Id == id);
    }

    public int DeleteByKey(string key) {
        return _repo.Delete(a => a.Key == key);
    }

    public string this[string key] {
        get {
            var item = GetByKey(key);
            return item == null ? "" : item.Value;
        }
        set {
            var item = GetByKey(key) ?? new ConfigItem {Key = key, Value = value};
            AddOrUpdate(item);
        }
    }
}