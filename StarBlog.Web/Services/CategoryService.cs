using FreeSql;
using StarBlog.Data.Models;

namespace StarBlog.Web.Services;

public class CategoryService {
    private readonly IBaseRepository<Category> _cRepo;
    private readonly IBaseRepository<FeaturedCategory> _fcRepo;

    public CategoryService(IBaseRepository<Category> cRepo, IBaseRepository<FeaturedCategory> fcRepo) {
        _cRepo = cRepo;
        _fcRepo = fcRepo;
    }

    public List<FeaturedCategory> GetFeaturedCategories() {
        return _fcRepo.Select.Include(a => a.Category).ToList();
    }
}