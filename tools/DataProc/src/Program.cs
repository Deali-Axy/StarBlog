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

// 可以选择运行不同的服务
Console.WriteLine("请选择要运行的服务:");
Console.WriteLine("1. VisitRecordService (访问记录处理)");
Console.WriteLine("2. ImageOptimizer (图片优化)");
Console.WriteLine("3. SlugGenerator (URL Slug 生成)");
Console.WriteLine("4. SummaryGenerator (文章摘要生成)");
Console.Write("请输入选择 (1-4): ");

var choice = Console.ReadLine();
switch (choice) {
    case "1":
        await app.Run<VisitRecordService>();
        break;
    case "2":
        await app.Run<ImageOptimizer>();
        break;
    case "3":
        await app.Run<SlugGenerator>();
        break;
    case "4":
        await app.Run<SummaryGenerator>();
        break;
    default:
        Console.WriteLine("无效选择，程序退出。");
        return;
}

Console.Read();