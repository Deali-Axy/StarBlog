// See https://aka.ms/new-console-template for more information

using IP2Region.Net.Abstractions;
using IP2Region.Net.XDB;
using DataProc.Services;
using DataProc.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarBlog.Data;
using StarBlog.Data.Models;
using StarBlog.Data.Extensions;

var builder = FluentConsoleApp.CreateBuilder(args);
var app = builder.Build();

builder.Services.AddDbContext<AppDbContext>(options => {
    options.UseSqlite(
        builder.Configuration.GetConnectionString("SQLite-Log"),
        opt => opt.MaxBatchSize(200)
    );
});

// 添加FreeSql支持
builder.Services.AddFreeSql(builder.Configuration);

// 添加IP2Region支持，如果文件不存在则使用假实现
var ip2RegionPath = app.Configuration.GetConnectionString("Ip2Region");
if (!string.IsNullOrEmpty(ip2RegionPath) && File.Exists(ip2RegionPath)) {
    app.Services.AddSingleton<ISearcher>(new Searcher(
        CachePolicy.Content,
        ip2RegionPath
    ));
}
else {
    app.Services.AddSingleton<ISearcher>(new FakeIpSearcher());
}

app.Services.AddAutoMapper(typeof(Program));

// 显示服务选择菜单
Console.WriteLine();
Console.WriteLine("           StarBlog 数据处理工具");
Console.WriteLine();
Console.WriteLine("请选择要运行的服务:");
Console.WriteLine();
Console.WriteLine("  1. VisitRecordService    - 访问记录处理");
Console.WriteLine("  2. ImageOptimizer        - 图片优化");
Console.WriteLine("  3. SlugGenerator         - URL Slug 生成");
Console.WriteLine("  4. SummaryGenerator      - 文章摘要生成");
Console.WriteLine("  5. DataInsightScript     - 数据洞察分析");
Console.WriteLine();
Console.WriteLine("  0. 退出程序");
Console.WriteLine();
Console.Write("请输入选择 (0-5): ");

var choice = Console.ReadLine()?.Trim();

Console.WriteLine();

switch (choice) {
    case "0":
        Console.WriteLine("程序已退出。");
        return;
    case "1":
        Console.WriteLine("正在启动访问记录处理服务...");
        await app.Run<VisitRecordService>();
        break;
    case "2":
        Console.WriteLine("正在启动图片优化服务...");
        await app.Run<ImageOptimizer>();
        break;
    case "3":
        Console.WriteLine("正在启动URL Slug生成服务...");
        await app.Run<SlugGenerator>();
        break;
    case "4":
        Console.WriteLine("正在启动文章摘要生成服务...");
        await app.Run<SummaryGenerator>();
        break;
    case "5":
        Console.WriteLine("正在启动数据洞察分析服务...");
        await app.Run<DataInsightScript>();
        break;
    default:
        Console.WriteLine($"无效选择: '{choice}'");
        Console.WriteLine("请输入 0-5 之间的数字。");
        Console.WriteLine("程序退出。");
        return;
}

Console.WriteLine();
Console.WriteLine("服务执行完成。");
Console.WriteLine("按任意键退出...");
Console.ReadKey();