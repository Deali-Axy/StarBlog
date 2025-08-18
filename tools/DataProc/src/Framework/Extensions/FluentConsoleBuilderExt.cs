using DataProc.Entities;
using DataProc.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DataProc.Framework.Extensions;

public static class FluentConsoleBuilderExt {
    public static FluentConsoleBuilder InitializeConfiguration(this FluentConsoleBuilder builder) {
        IConfigurationRoot? config;
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddEnvironmentVariables();
        configBuilder.SetBasePath(Environment.CurrentDirectory);
        configBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
        try {
            config = configBuilder.Build();
        }
        catch (Exception ex) {
            Console.WriteLine($"配置文件加载失败！请检查配置文件是不是哪里写错了？\n错误信息：{ex.Message}");
            throw;
        }

        builder.Configuration = config;
        builder.Services.AddSingleton<IConfiguration>(config);
        builder.Services.AddOptions().Configure<AppSettings>(e => config.GetSection(nameof(AppSettings)).Bind(e));

        return builder;
    }

    public static FluentConsoleBuilder InitializeLogging(this FluentConsoleBuilder builder) {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("logs/fluent-demo-logs.log")
            .CreateLogger();

        builder.Services.AddLogging(b => {
            b.AddConfiguration(builder.Configuration.GetSection("Logging"));
            b.AddConsole();
            b.AddSerilog(dispose: true);
        });

        return builder;
    }

    public static FluentConsoleBuilder RegisterServices(this FluentConsoleBuilder builder) {
        var serviceTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IService).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

        foreach (var type in serviceTypes) {
            builder.Services.AddSingleton(type);
        }

        return builder;
    }
}