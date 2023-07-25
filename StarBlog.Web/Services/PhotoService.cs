using AutoMapper;
using FreeSql;
using ImageMagick;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using StarBlog.Share.Utils;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels.Photography;
using X.PagedList;

namespace StarBlog.Web.Services;

public class PhotoService {
    private readonly IBaseRepository<Photo> _photoRepo;
    private readonly IBaseRepository<FeaturedPhoto> _featuredPhotoRepo;
    private readonly IWebHostEnvironment _environment;
    private readonly IMapper _mapper;

    public PhotoService(IBaseRepository<Photo> photoRepo, IWebHostEnvironment environment,
        IBaseRepository<FeaturedPhoto> featuredPhotoRepo, IMapper mapper) {
        _photoRepo = photoRepo;
        _environment = environment;
        _featuredPhotoRepo = featuredPhotoRepo;
        _mapper = mapper;
    }

    public async Task<List<Photo>> GetAll() {
        return await _photoRepo.Select.ToListAsync();
    }

    public async Task<IPagedList<Photo>> GetPagedList(int page = 1, int pageSize = 10) {
        return (await _photoRepo.Select.OrderByDescending(a => a.CreateTime)
            .ToListAsync()).ToPagedList(page, pageSize);
    }

    public async Task<List<Photo>> GetFeaturedPhotos() {
        return await _featuredPhotoRepo.Select
            .Include(a => a.Photo).ToListAsync(a => a.Photo);
    }

    public async Task<Photo?> GetById(string id) {
        return await _photoRepo.Where(a => a.Id == id).FirstAsync();
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
    /// <param name="id"></param>
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

    public async Task<Photo> Update(PhotoUpdateDto dto) {
        var photo = await GetById(dto.Id);
        photo = _mapper.Map(dto, photo);
        await _photoRepo.UpdateAsync(photo);
        return photo;
    }

    public async Task<Photo> Add(PhotoCreationDto dto, IFormFile photoFile) {
        var photoId = GuidUtils.GuidTo16String();
        var photo = new Photo {
            Id = photoId,
            Title = dto.Title,
            CreateTime = DateTime.Now,
            Location = dto.Location,
            FilePath = $"{photoId}.jpg"
        };

        var savePath = GetPhotoFilePath(photo);

        // 如果图片超出大小限制则需要先调整
        var resizeFlag = await ResizePhoto(photoFile.OpenReadStream(), savePath);

        // 没调整过大小则直接保存上传的图片
        if (!resizeFlag) {
            await using (var fs = new FileStream(savePath, FileMode.Create)) {
                await photoFile.CopyToAsync(fs);
            }
        }

        photo = await BuildPhotoData(photo);

        return await _photoRepo.InsertAsync(photo);
    }

    /// <summary>
    /// 获取随机一张图片
    /// </summary>
    public async Task<Photo?> GetRandomPhoto() {
        var count = await _photoRepo.Select.CountAsync();
        if (count == 0) {
            return null;
        }

        return await _photoRepo.Select.Take(1).Offset(Random.Shared.Next((int)count)).FirstAsync();
    }

    /// <summary>
    /// 添加推荐图片
    /// </summary>
    public async Task<FeaturedPhoto> AddFeaturedPhoto(Photo photo) {
        var item = await _featuredPhotoRepo.Where(a => a.PhotoId == photo.Id).FirstAsync();
        if (item != null) return item;
        item = new FeaturedPhoto { PhotoId = photo.Id };
        await _featuredPhotoRepo.InsertAsync(item);
        return item;
    }

    /// <summary>
    /// 删除推荐图片
    /// </summary>
    public async Task<int> DeleteFeaturedPhoto(Photo photo) {
        var item = await _featuredPhotoRepo.Where(a => a.PhotoId == photo.Id).FirstAsync();
        return item == null ? 0 : await _featuredPhotoRepo.DeleteAsync(item);
    }

    /// <summary>
    /// 删除照片
    /// <para>删除照片文件和数据库记录</para>
    /// </summary>
    public async Task<int> DeleteById(string id) {
        var photo = await _photoRepo.Where(a => a.Id == id).FirstAsync();
        if (photo == null) return -1;

        var filePath = GetPhotoFilePath(photo);
        if (File.Exists(filePath)) File.Delete(filePath);
        return await _photoRepo.DeleteAsync(a => a.Id == id);
    }

    /// <summary>
    /// 重建图片库数据（重新扫描每张图片的大小等数据）
    /// </summary>
    public async Task<int> ReBuildData() {
        var photos = await GetAll();
        var photosUpdate = new List<Photo>();
        foreach (var photo in photos) {
            photosUpdate.Add(await BuildPhotoData(photo));
        }

        return await _photoRepo.UpdateAsync(photosUpdate);
    }

    /// <summary>
    /// 批量导入图片
    /// </summary>
    public async Task<List<Photo>> BatchImport() {
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
                FilePath = $"{photoId}.jpg"
            };
            var savePath = GetPhotoFilePath(photo);

            // 如果图片超出大小限制则需要先调整
            var resizeFlag = await ResizePhoto(new FileStream(file.FullName, FileMode.Open), savePath);

            // 没调整过大小则直接保存上传的图片
            if (!resizeFlag) {
                file.CopyTo(savePath, true);
            }

            photo = await BuildPhotoData(photo);
            await _photoRepo.InsertAsync(photo);
            result.Add(photo);
        }

        return result;
    }

    /// <summary>
    /// 初始化照片资源目录
    /// </summary>
    private string InitPhotoMediaDir() {
        var dir = Path.Combine(_environment.WebRootPath, "media", "photography");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        return dir;
    }

    /// <summary>
    /// 获取图片的物理存储路径
    /// </summary>
    private string GetPhotoFilePath(Photo photo) {
        return Path.Combine(InitPhotoMediaDir(), photo.FilePath);
    }

    /// <summary>
    /// 重建图片数据（扫描图片的大小等数据）
    /// </summary>
    private async Task<Photo> BuildPhotoData(Photo photo) {
        var savePath = GetPhotoFilePath(photo);
        var imgInfo = await Image.IdentifyAsync(savePath);
        photo.Width = imgInfo.Width;
        photo.Height = imgInfo.Height;

        return photo;
    }

    /// <summary>
    /// 根据设置调整图片大小
    /// </summary>
    private static async Task<bool> ResizePhoto(Stream stream, string savePath) {
        const int maxWidth = 1500;
        const int maxHeight = 1500;
        var resizeFlag = false;

        using var image = await Image.LoadAsync(stream);

        if (image.Width > maxWidth) {
            resizeFlag = true;
            image.Mutate(a => a.Resize(maxWidth, 0));
        }

        if (image.Height > maxHeight) {
            resizeFlag = true;
            image.Mutate(a => a.Resize(0, maxHeight));
        }

        if (resizeFlag) {
            await image.SaveAsync(savePath);
        }

        return resizeFlag;
    }
}