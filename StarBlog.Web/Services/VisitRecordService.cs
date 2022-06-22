using FreeSql;
using StarBlog.Data.Models;

namespace StarBlog.Web.Services;

public class VisitRecordService {
    private readonly IBaseRepository<VisitRecord> _repo;

    public VisitRecordService(IBaseRepository<VisitRecord> repo) {
        _repo = repo;
    }

    public VisitRecord? GetById(int id) {
        var item = _repo.Where(a => a.Id == id).First();
        return item;
    }

    public List<VisitRecord> GetAll() {
        return _repo.Select.ToList();
    }
}