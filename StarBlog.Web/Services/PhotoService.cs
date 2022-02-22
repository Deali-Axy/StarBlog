using FreeSql;
using StarBlog.Data.Models;
using X.PagedList;

namespace StarBlog.Web.Services;

public class PhotoService {
    private readonly IBaseRepository<Photo> _photoRepo;

    public PhotoService(IBaseRepository<Photo> photoRepo) {
        _photoRepo = photoRepo;
    }

    public IPagedList<Photo> GetPagedList(int page = 1, int pageSize = 10) {
        return _photoRepo.Select.ToList().ToPagedList(page, pageSize);
    }

    public Photo? GetById(string id) {
        return _photoRepo.Where(a => a.Id == id).First();
    }

    public int DeleteById(string id) {
        return _photoRepo.Delete(a => a.Id == id);
    }
}