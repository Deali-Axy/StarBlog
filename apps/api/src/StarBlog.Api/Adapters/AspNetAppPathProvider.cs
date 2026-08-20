using StarBlog.Application.Abstractions;

namespace StarBlog.Api.Adapters;

public sealed class AspNetAppPathProvider : IAppPathProvider {
    private readonly IWebHostEnvironment _env;

    public AspNetAppPathProvider(IWebHostEnvironment env) {
        _env = env;
    }

    public string WebRootPath => _env.WebRootPath;
}
