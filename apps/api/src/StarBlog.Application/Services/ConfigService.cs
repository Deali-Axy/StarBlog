using FreeSql;
using StarBlog.Data.Models;

namespace StarBlog.Application.Services;

public class ConfigService {
    private readonly IConfiguration _conf;
    private readonly IBaseRepository<ConfigItem> _repo;

    public ConfigService(IBaseRepository<ConfigItem> repo, IConfiguration conf) {
        _repo = repo;
        _conf = conf;
    }

    public List<ConfigItem> GetAll() {
        return _repo.Select.ToList();
    }

    public ConfigItem? GetById(int id) {
        return _repo.Where(a => a.Id == id).First();
    }

    public ConfigItem? GetByKey(string key) {
        var item = _repo.Where(a => a.Key == key).First();
        if (item == null) {
            // 尝试读取初始化配置
            var section = _conf.GetSection($"StarBlog:Initial:{key}");
            if (!section.Exists()) return null;
            item = new ConfigItem { Key = key, Value = section.Value, Description = "Initial" };
            item = AddOrUpdate(item);
        }

        return item;
    }

    public ConfigItem AddOrUpdate(ConfigItem item) {
        return _repo.InsertOrUpdate(item);
    }

    public int? Update(string key, string value, string? description = default) {
        var item = GetByKey(key);
        if (item == null) return null;

        item.Value = value;
        if (description != null) item.Description = description;
        return _repo.Update(item);
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
            var item = GetByKey(key) ?? new ConfigItem { Key = key };
            item.Value = value;
            AddOrUpdate(item);
        }
    }
}