using FreeSql;
using SixLabors.ImageSharp;
using StarBlog.Contrib.Utils;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels.Photography;
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

    public string GetPhotoFilePath(Photo photo) {
        return Path.Combine(_environment.WebRootPath, "media", photo.FilePath);
    }

    public Photo Add(PhotoCreationDto dto, IFormFile photoFile) {
        var photoId = GuidUtils.GuidTo16String();
        var photo = new Photo {
            Id = photoId,
            Title = dto.Title,
            CreateTime = DateTime.Now,
            Location = dto.Location,
            FilePath = Path.Combine("photography", $"{photoId}.jpg")
        };

        var savePath = GetPhotoFilePath(photo);
        using (var fs = new FileStream(savePath, FileMode.Create)) {
            photoFile.CopyTo(fs);
        }

        using (var img = Image.Load(savePath)) {
            photo.Height = img.Height;
            photo.Width = img.Width;
        }


        return _photoRepo.Insert(photo);
    }

    /// <summary>
    /// 删除照片
    /// <para>删除照片文件和数据库记录</para>
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public int DeleteById(string id) {
        var photo = _photoRepo.Where(a => a.Id == id).First();
        if (photo == null) return -1;

        var filePath = GetPhotoFilePath(photo);
        if (File.Exists(filePath)) File.Delete(filePath);
        return _photoRepo.Delete(a => a.Id == id);
    }
}