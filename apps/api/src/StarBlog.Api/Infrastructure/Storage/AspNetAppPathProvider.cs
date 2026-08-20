namespace StarBlog.Api.Infrastructure.Storage;

/// <summary>
/// 从 ASP.NET Core 宿主环境读取 WebRoot。
/// </summary>
public sealed class AspNetAppPathProvider : IAppPathProvider {
    private readonly IWebHostEnvironment _environment;

    public AspNetAppPathProvider(IWebHostEnvironment environment) {
        _environment = environment;
    }

    public string WebRootPath => _environment.WebRootPath;
}
