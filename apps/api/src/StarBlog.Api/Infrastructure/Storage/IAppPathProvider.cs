namespace StarBlog.Api.Infrastructure.Storage;

/// <summary>
/// 提供 Web 根路径，供文件存储把相对路径映射到物理目录。
/// </summary>
public interface IAppPathProvider {
    string WebRootPath { get; }
}
