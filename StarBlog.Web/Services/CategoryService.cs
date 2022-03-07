using FreeSql;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels.Categories;
using X.PagedList;

namespace StarBlog.Web.Services;

public class CategoryService {
    private readonly IBaseRepository<Category> _cRepo;
    private readonly IBaseRepository<FeaturedCategory> _fcRepo;

    public CategoryService(IBaseRepository<Category> cRepo, IBaseRepository<FeaturedCategory> fcRepo) {
        _cRepo = cRepo;
        _fcRepo = fcRepo;
    }

    public List<Category> GetAll() {
        return _cRepo.Select.ToList();
    }

    public IPagedList<Category> GetPagedList(int page = 1, int pageSize = 10) {
        return _cRepo.Select.ToList().ToPagedList(page, pageSize);
    }

    public Category? GetById(int id) {
        return _cRepo.Where(a => a.Id == id).First();
    }

    /// <summary>
    /// 生成分类词云数据
    /// </summary>
    /// <returns></returns>
    public List<object> GetWordCloud() {
        var list = _cRepo.Select.IncludeMany(a => a.Posts).ToList();
        var data = new List<object>();
        foreach (var item in list) {
            data.Add(new {name = item.Name, value = item.Posts.Count});
        }

        return data;
    }

    public List<FeaturedCategory> GetFeaturedCategories() {
        return _fcRepo.Select.Include(a => a.Category).ToList();
    }

    public FeaturedCategory? GetFeaturedCategoryById(int id) {
        return _fcRepo.Where(a => a.Id == id)
            .Include(a => a.Category).First();
    }

    public FeaturedCategory AddOrUpdateFeaturedCategory(Category category, FeaturedCategoryCreationDto dto) {
        var item = _fcRepo.Where(a => a.CategoryId == category.Id).First();
        if (item == null) {
            item = new FeaturedCategory {
                CategoryId = category.Id,
                Name = dto.Name,
                Description = dto.Description,
                IconCssClass = dto.IconCssClass
            };
        }
        else {
            item.Name = dto.Name;
            item.Description = dto.Description;
            item.IconCssClass = dto.IconCssClass;
        }

        _fcRepo.InsertOrUpdate(item);
        return item;
    }

    public int DeleteFeaturedCategory(Category category) {
        var item = _fcRepo.Where(a => a.CategoryId == category.Id).First();
        return item == null ? 0 : _fcRepo.Delete(item);
    }

    public int DeleteFeaturedCategoryById(int id) {
        return _fcRepo.Delete(a => a.Id == id);
    }

    public int Setvisibility(Category category, bool isVisible) {
        category.Visible = isVisible;
        return _cRepo.Update(category);
    }
}