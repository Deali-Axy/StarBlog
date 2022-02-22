using FreeSql;
using StarBlog.Contrib.Utils;
using StarBlog.Data.Models;
using X.PagedList;

namespace StarBlog.Web.Services;

public class PhotoService {
    private readonly IBaseRepository<Photo> _photoRepo;
    private readonly IWebHostEnvironment _environment;

    public PhotoService(IBaseRepository<Photo> photoRepo, IWebHostEnvironment environment) {
        _photoRepo = photoRepo;
        _environment = environment;
    }

    public IPagedList<Photo> GetPagedList(int page = 1, int pageSize = 10) {
        return _photoRepo.Select.ToList().ToPagedList(page, pageSize);
    }

    public Photo? GetById(string id) {
        return _photoRepo.Where(a => a.Id == id).First();
    }

    public Photo Add(string title, IFormFile photoFile) {
        var photoId = GuidUtils.GuidTo16String();
        var photo = new Photo {
            Id = photoId,
            Title = title,
            CreateTime = DateTime.Now,
            Location = Path.Combine("photography", $"{photoId}.jpg")
        };

        var savePath = Path.Combine(_environment.WebRootPath, "media", photo.Location);
        photoFile.CopyTo(new FileStream(savePath, FileMode.Create));
        
        return _photoRepo.Insert(photo);
    }

    public int DeleteById(string id) {
        return _photoRepo.Delete(a => a.Id == id);
    }
}