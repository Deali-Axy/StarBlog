using FreeSql;
using SixLabors.ImageSharp;
using StarBlog.Contrib.Utils;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels.Photography;
using X.PagedList;

namespace StarBlog.Web.Services;

public class PhotoService {
    private readonly IBaseRepository<Photo> _photoRepo;
    private readonly IBaseRepository<FeaturedPhoto> _featuredPhotoRepo;
    private readonly IWebHostEnvironment _environment;

    public PhotoService(IBaseRepository<Photo> photoRepo, IWebHostEnvironment environment, IBaseRepository<FeaturedPhoto> featuredPhotoRepo) {
        _photoRepo = photoRepo;
        _environment = environment;
        _featuredPhotoRepo = featuredPhotoRepo;
    }

    public List<Photo> GetAll() {
        return _photoRepo.Select.ToList();
    }

    public IPagedList<Photo> GetPagedList(int page = 1, int pageSize = 10) {
        return _photoRepo.Select.ToList().ToPagedList(page, pageSize);
    }

    public List<Photo> GetFeaturedPhotos() {
        return _featuredPhotoRepo.Select
            .Include(a => a.Photo).ToList(a => a.Photo);
    }

    public Photo? GetById(string id) {
        return _photoRepo.Where(a => a.Id == id).First();
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

        photo = BuildPhotoData(photo);

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

    /// <summary>
    /// 重建图片库数据（重新扫描每张图片的大小等数据）
    /// </summary>
    public int ReBuildData() {
        var photos = GetAll();
        return photos.Sum(photo => _photoRepo.Update(BuildPhotoData(photo)));
    }

    /// <summary>
    /// 批量导入图片
    /// </summary>
    /// <returns></returns>
    public List<Photo> BatchImport() {
        var result = new List<Photo>();
        var importPath = Path.Combine(_environment.WebRootPath, "assets", "photography");
        var root = new DirectoryInfo(importPath);
        foreach (var file in root.GetFiles()) {
            var photoId = GuidUtils.GuidTo16String();
            var filename = Path.GetFileNameWithoutExtension(file.Name);
            var photo = new Photo {
                Id = photoId,
                Title = filename,
                CreateTime = DateTime.Now,
                Location = filename,
                FilePath = Path.Combine("photography", $"{photoId}.jpg")
            };
            var savePath = GetPhotoFilePath(photo);
            file.CopyTo(savePath, true);
            photo = BuildPhotoData(photo);
            _photoRepo.Insert(photo);
            result.Add(photo);
        }

        return result;
    }

    private string GetPhotoFilePath(Photo photo) {
        return Path.Combine(_environment.WebRootPath, "media", photo.FilePath);
    }

    /// <summary>
    /// 重建图片数据（扫描图片的大小等数据）
    /// </summary>
    /// <param name="photo"></param>
    /// <returns></returns>
    private Photo BuildPhotoData(Photo photo) {
        var savePath = GetPhotoFilePath(photo);
        using (var img = Image.Load(savePath)) {
            photo.Height = img.Height;
            photo.Width = img.Width;
        }

        return photo;
    }
}