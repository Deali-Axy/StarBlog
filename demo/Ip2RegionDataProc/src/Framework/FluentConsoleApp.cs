using System.Reflection;
using dotenv.net;
using Ip2RegionDataProc.Framework.Extensions;
using Ip2RegionDataProc.Services;
using Ip2RegionDataProc.Utilities;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ip2RegionDataProc.Framework;

public sealed class FluentConsoleApp {
    public static FluentConsoleBuilder CreateBuilder(string[] args) {
        DotEnv.Load();

        var version = Assembly.GetExecutingAssembly().GetName().Version;

        ConsoleTool.PrintLogo();
        ConsoleTool.PrintTitle($"Ip2RegionDataProc - {version}");

        var builder = new FluentConsoleBuilder {
            Services = new ServiceCollection()
        };

        builder.InitializeConfiguration()
            .InitializeLogging()
            .RegisterServices();

        return builder;
    }

    public IConfiguration Configuration { get; set; }
    public IServiceCollection Services { get; set; }

    internal FluentConsoleApp() { }

    /// <summary>
    /// 运行指定任务
    /// </summary>
    public async Task<Result> Run<T>() where T : IService {
        await using var sp = Services.BuildServiceProvider();
        await using var scope = sp.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<T>();
        return await service.Run();
    }
}