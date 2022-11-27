using FreeSql;
using ImageMagick;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using StarBlog.Contrib.Utils;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels.Photography;
using X.PagedList;

namespace StarBlog.Web.Services;

public class PhotoService {
    private readonly IBaseRepository<Photo> _photoRepo;
    private readonly IBaseRepository<FeaturedPhoto> _featuredPhotoRepo;
    private readonly IWebHostEnvironment _environment;

    public PhotoService(IBaseRepository<Photo> photoRepo, IWebHostEnvironment environment,
        IBaseRepository<FeaturedPhoto> featuredPhotoRepo) {
        _photoRepo = photoRepo;
        _environment = environment;
        _featuredPhotoRepo = featuredPhotoRepo;
    }

    public List<Photo> GetAll() {
        return _photoRepo.Select.ToList();
    }

    public IPagedList<Photo> GetPagedList(int page = 1, int pageSize = 10) {
        return _photoRepo.Select.OrderByDescending(a => a.CreateTime)
            .ToList().ToPagedList(page, pageSize);
    }

    public List<Photo> GetFeaturedPhotos() {
        return _featuredPhotoRepo.Select
            .Include(a => a.Photo).ToList(a => a.Photo);
    }

    public Photo? GetById(string id) {
        return _photoRepo.Where(a => a.Id == id).First();
    }

    public async Task<Photo?> GetNext(string id) {
        var photo = await _photoRepo.Where(a => a.Id == id).FirstAsync();
        if (photo == null) return null;
        var next = await _photoRepo
            .Where(a => a.CreateTime < photo.CreateTime && a.Id != id)
            .OrderByDescending(a => a.CreateTime)
            .FirstAsync();
        return next;
    }

    public async Task<Photo?> GetPrevious(string id) {
        var photo = await _photoRepo.Where(a => a.Id == id).FirstAsync();
        if (photo == null) return null;
        var next = await _photoRepo
            .Where(a => a.CreateTime > photo.CreateTime && a.Id != id)
            .OrderBy(a => a.CreateTime)
            .FirstAsync();
        return next;
    }

    /// <summary>
    /// 生成Progressive JPEG缩略图 （使用 MagickImage）
    /// </summary>
    /// <param name="width">设置为0则不调整大小</param>
    public async Task<byte[]> GetThumb(string id, int width = 0) {
        var photo = await _photoRepo.Where(a => a.Id == id).FirstAsync();
        using (var image = new MagickImage(GetPhotoFilePath(photo))) {
            image.Format = MagickFormat.Pjpeg;
            if (width != 0) {
                image.Resize(width, 0);
            }

            return image.ToByteArray();
        }
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

        const int maxWidth = 2000;
        const int maxHeight = 2000;
        using (var image = Image.Load(photoFile.OpenReadStream())) {
            if (image.Width > maxWidth)
                image.Mutate(a => a.Resize(maxWidth, 0));
            if (image.Height > maxHeight)
                image.Mutate(a => a.Resize(0, maxHeight));
            image.Save(savePath);
        }

        using (var fs = new FileStream(savePath, FileMode.Create)) {
            photoFile.CopyTo(fs);
        }

        photo = BuildPhotoData(photo);

        return _photoRepo.Insert(photo);
    }

    /// <summary>
    /// 获取随机一张图片
    /// </summary>
    /// <returns></returns>
    public Photo? GetRandomPhoto() {
        var items = GetAll();
        return items.Count == 0 ? null : items[new Random().Next(items.Count)];
    }

    /// <summary>
    /// 添加推荐图片
    /// </summary>
    /// <param name="photo"></param>
    /// <returns></returns>
    public FeaturedPhoto AddFeaturedPhoto(Photo photo) {
        var item = _featuredPhotoRepo.Where(a => a.PhotoId == photo.Id).First();
        if (item != null) return item;
        item = new FeaturedPhoto {PhotoId = photo.Id};
        _featuredPhotoRepo.Insert(item);
        return item;
    }

    /// <summary>
    /// 删除推荐图片
    /// </summary>
    /// <param name="photo"></param>
    /// <returns></returns>
    public int DeleteFeaturedPhoto(Photo photo) {
        var item = _featuredPhotoRepo.Where(a => a.PhotoId == photo.Id).First();
        return item == null ? 0 : _featuredPhotoRepo.Delete(item);
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

    /// <summary>
    /// 获取图片的物理存储路径
    /// </summary>
    /// <param name="photo"></param>
    /// <returns></returns>
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
        var imgInfo = Image.Identify(savePath);
        photo.Width = imgInfo.Width;
        photo.Height = imgInfo.Height;

        return photo;
    }
}