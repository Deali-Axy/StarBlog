﻿// See https://aka.ms/new-console-template for more information

using IP2Region.Net.Abstractions;
using IP2Region.Net.XDB;
using Ip2RegionDataProc.Services;
using Ip2RegionDataProc.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarBlog.Data;
using StarBlog.Data.Models;

var builder = FluentConsoleApp.CreateBuilder(args);
var app = builder.Build();

builder.Services.AddDbContext<AppDbContext>(options => {
    options.UseSqlite(
        builder.Configuration.GetConnectionString("SQLite-Log"),
        opt => opt.MaxBatchSize(200)
    );
});
app.Services.AddSingleton<ISearcher>(new Searcher(
    CachePolicy.Content,
    app.Configuration.GetConnectionString("Ip2Region")!
));

await app.Run<MainService>();

Console.Read();