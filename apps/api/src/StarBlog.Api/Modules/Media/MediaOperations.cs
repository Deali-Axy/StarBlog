using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Infrastructure.Storage;
using StarBlog.Api.Infrastructure.Time;
using StarBlog.Api.Modules.Media.Domain;
using StarBlog.Api.Shared;

namespace StarBlog.Api.Modules.Media;

/// <summary>
/// 图片上传、缩略图、精选与相邻导航。文件写入失败时不提交数据库记录。
/// </summary>
public sealed class MediaOperations : IPhotoCatalog {
    private const string MediaDirectory = "media/photography";
    private readonly StarBlogDbContext _db;
    private readonly IFileStorage _storage;
    private readonly IAppPathProvider _paths;
    private readonly IClock _clock;

    public MediaOperations(StarBlogDbContext db, IFileStorage storage, IAppPathProvider paths, IClock clock) {
        _db = db;
        _storage = storage;
        _paths = paths;
        _clock = clock;
    }

    /// <summary>分页列出全部图片，按创建时间倒序。</summary>
    public async Task<PageResult<PhotoResponse>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken) {
        page = Paging.NormalizePage(page);
        pageSize = Paging.NormalizePageSize(pageSize);
        var query = _db.Photos.AsNoTracking().OrderByDescending(photo => photo.CreateTime);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PageResult<PhotoResponse> {
            Items = items.Select(PhotoResponse.From).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<PhotoResponse?> GetByIdAsync(string id, CancellationToken cancellationToken) {
        var photo = await _db.Photos.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return photo == null ? null : PhotoResponse.From(photo);
    }

    /// <summary>先编码到内存再写存储并插入数据库；任一步失败时删除已写入的文件，避免空记录或孤儿文件。</summary>
    public async Task<PhotoResponse> AddAsync(string title, string location, Stream photoStream, CancellationToken cancellationToken) {
        var photoId = Guid.NewGuid().ToString("N")[..16];
        var fileName = $"{photoId}.jpg";
        var relativePath = $"{MediaDirectory}/{fileName}";
        var photo = new Photo {
            Id = photoId,
            Title = title,
            Location = location,
            FilePath = fileName,
            CreateTime = _clock.Now
        };

        await using var buffered = new MemoryStream();
        await photoStream.CopyToAsync(buffered, cancellationToken);
        buffered.Position = 0;
        await using var encoded = new MemoryStream();
        await EncodeAsJpegAsync(buffered, encoded);
        encoded.Position = 0;

        await _storage.EnsureDirectoryAsync(MediaDirectory, cancellationToken);
        try {
            await _storage.SaveAsync(relativePath, encoded, cancellationToken: cancellationToken);
            await FillDimensionsAsync(photo);
            _db.Photos.Add(photo);
            await _db.SaveChangesAsync(cancellationToken);
            return PhotoResponse.From(photo);
        }
        catch {
            await _storage.DeleteAsync(relativePath, cancellationToken);
            throw;
        }
    }

    public async Task<PhotoResponse?> UpdateAsync(string id, PhotoUpdateRequest request, CancellationToken cancellationToken) {
        var photo = await _db.Photos.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (photo == null) return null;
        photo.Title = request.Title;
        photo.Location = request.Location;
        await _db.SaveChangesAsync(cancellationToken);
        return PhotoResponse.From(photo);
    }

    /// <summary>先删推荐记录和文件，再删数据库。文件删除失败时仍删除记录，避免悬挂元数据。</summary>
    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken) {
        var photo = await _db.Photos.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (photo == null) return false;

        var featured = await _db.FeaturedPhotos.Where(item => item.PhotoId == id).ToListAsync(cancellationToken);
        _db.FeaturedPhotos.RemoveRange(featured);
        await _storage.DeleteAsync($"{MediaDirectory}/{photo.FilePath}", cancellationToken);
        _db.Photos.Remove(photo);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>生成 JPEG 缩略图。</summary>
    public async Task<byte[]?> GetThumbAsync(string id, int width, int quality, CancellationToken cancellationToken) {
        var photo = await _db.Photos.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (photo == null) return null;
        var path = PhysicalPath(photo);
        if (!File.Exists(path)) return null;
        using var image = await Image.LoadAsync(path, cancellationToken);
        if (width > 0) image.Mutate(context => context.Resize(width, 0));
        await using var memory = new MemoryStream();
        await image.SaveAsync(memory, new JpegEncoder { Quality = quality }, cancellationToken);
        return memory.ToArray();
    }

    public async Task<FeaturedPhotoResponse?> SetFeaturedAsync(string id, CancellationToken cancellationToken) {
        var photo = await _db.Photos.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (photo == null) return null;
        var existing = await _db.FeaturedPhotos.Include(item => item.Photo)
            .FirstOrDefaultAsync(item => item.PhotoId == id, cancellationToken);
        if (existing != null) {
            return new FeaturedPhotoResponse { Id = existing.Id, PhotoId = existing.PhotoId, Photo = PhotoResponse.From(existing.Photo) };
        }

        var featured = new FeaturedPhoto { PhotoId = id, Photo = photo };
        _db.FeaturedPhotos.Add(featured);
        await _db.SaveChangesAsync(cancellationToken);
        return new FeaturedPhotoResponse { Id = featured.Id, PhotoId = featured.PhotoId, Photo = PhotoResponse.From(photo) };
    }

    public async Task<bool> CancelFeaturedAsync(string id, CancellationToken cancellationToken) {
        var featured = await _db.FeaturedPhotos.FirstOrDefaultAsync(item => item.PhotoId == id, cancellationToken);
        if (featured == null) return false;
        _db.FeaturedPhotos.Remove(featured);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<PhotoResponse?> GetRandomAsync(CancellationToken cancellationToken = default) {
        var count = await _db.Photos.CountAsync(cancellationToken);
        if (count == 0) return null;
        var photo = await _db.Photos.AsNoTracking().OrderBy(item => item.Id).Skip(Random.Shared.Next(count)).FirstAsync(cancellationToken);
        return PhotoResponse.From(photo);
    }

    public async Task<PhotoResponse?> GetNextAsync(string id, CancellationToken cancellationToken = default) {
        var current = await _db.Photos.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (current == null) return null;
        var next = await _db.Photos.AsNoTracking()
            .Where(item => item.CreateTime < current.CreateTime && item.Id != id)
            .OrderByDescending(item => item.CreateTime)
            .FirstOrDefaultAsync(cancellationToken);
        return next == null ? null : PhotoResponse.From(next);
    }

    public async Task<PhotoResponse?> GetPreviousAsync(string id, CancellationToken cancellationToken = default) {
        var current = await _db.Photos.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (current == null) return null;
        var previous = await _db.Photos.AsNoTracking()
            .Where(item => item.CreateTime > current.CreateTime && item.Id != id)
            .OrderBy(item => item.CreateTime)
            .FirstOrDefaultAsync(cancellationToken);
        return previous == null ? null : PhotoResponse.From(previous);
    }

    public async Task<IReadOnlyList<PhotoResponse>> GetFeaturedAsync(CancellationToken cancellationToken = default) {
        var photos = await _db.FeaturedPhotos.AsNoTracking().Include(item => item.Photo)
            .Select(item => item.Photo)
            .ToListAsync(cancellationToken);
        return photos.Select(PhotoResponse.From).ToList();
    }

    /// <summary>全部图片数量，供 Site 聚合概况使用。</summary>
    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        _db.Photos.CountAsync(cancellationToken);

    /// <summary>精选图片数量，供 Site 聚合概况使用。</summary>
    public Task<int> CountFeaturedAsync(CancellationToken cancellationToken = default) =>
        _db.FeaturedPhotos.CountAsync(cancellationToken);

    private string PhysicalPath(Photo photo) => Path.Combine(_paths.WebRootPath, "media", "photography", photo.FilePath);

    private async Task FillDimensionsAsync(Photo photo) {
        var info = await Image.IdentifyAsync(PhysicalPath(photo));
        photo.Width = info.Width;
        photo.Height = info.Height;
    }

    /// <summary>把任意可解码图片转成边长不超过 1500 的 JPEG，写入完成后再落盘。</summary>
    private static async Task EncodeAsJpegAsync(Stream stream, Stream output) {
        const int max = 1500;
        using var image = await Image.LoadAsync(stream);
        if (image.Width > max) {
            image.Mutate(context => context.Resize(max, 0));
        }

        if (image.Height > max) {
            image.Mutate(context => context.Resize(0, max));
        }

        await image.SaveAsJpegAsync(output);
    }
}
