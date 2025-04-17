using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ip2RegionDataProc.Framework;

public class FluentConsoleBuilder {
    public IConfiguration Configuration { get; set; }
    public IServiceCollection Services { get; set; }

    internal FluentConsoleBuilder() { }

    public FluentConsoleApp Build() {
        var app = new FluentConsoleApp {
            Configuration = Configuration,
            Services = Services
        };

        return app;
    }
}