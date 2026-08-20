using System.ComponentModel.DataAnnotations;
using StarBlog.Api.Modules.Media.Domain;

namespace StarBlog.Api.Modules.Media;

/// <summary>图片对外契约，不暴露物理路径细节以外的存储实现。</summary>
public sealed class PhotoResponse {
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Location { get; init; }
    public required string FilePath { get; init; }
    public long Width { get; init; }
    public long Height { get; init; }
    public DateTime CreateTime { get; init; }

    public static PhotoResponse From(Photo photo) => new() {
        Id = photo.Id,
        Title = photo.Title,
        Location = photo.Location,
        FilePath = photo.FilePath,
        Width = photo.Width,
        Height = photo.Height,
        CreateTime = photo.CreateTime
    };
}

/// <summary>更新图片元数据。</summary>
public sealed class PhotoUpdateRequest {
    [Required, StringLength(256)]
    public string Title { get; init; } = string.Empty;

    [Required, StringLength(256)]
    public string Location { get; init; } = string.Empty;
}

/// <summary>推荐图片结果。</summary>
public sealed class FeaturedPhotoResponse {
    public int Id { get; init; }
    public required string PhotoId { get; init; }
    public PhotoResponse? Photo { get; init; }
}

/// <summary>Site 模块读取图片的只读契约。</summary>
public interface IPhotoCatalog {
    Task<PhotoResponse?> GetRandomAsync(CancellationToken cancellationToken = default);
    Task<PhotoResponse?> GetNextAsync(string id, CancellationToken cancellationToken = default);
    Task<PhotoResponse?> GetPreviousAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PhotoResponse>> GetFeaturedAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountFeaturedAsync(CancellationToken cancellationToken = default);
}
