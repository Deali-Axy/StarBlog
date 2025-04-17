using System.IO;
using System.Text.Json;
using Dumpify;
using Ip2RegionDataProc.Entities;
using Ip2RegionDataProc.Utilities;
using FluentResults;
using IP2Region.Net.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarBlog.Data;
using StarBlog.Data.Models;

namespace Ip2RegionDataProc.Services;

public class MainService(
    ILogger<MainService> logger,
    IOptions<AppSettings> options,
    IConfiguration conf,
    ISearcher searcher,
    AppDbContext db)
    : IService {
    private readonly AppSettings _settings = options.Value;
    private readonly IConfiguration _conf = conf;

    public async Task<Result> Run() {
        logger.LogInformation("启动！");

        var records = await db.VisitRecords.ToListAsync();
        logger.LogInformation($"已读取 {records.Count} 条访问日志，正在处理IP地址");
        var recordsToUpdate = records.Select(InflateIpRegion).ToList();
        logger.LogInformation($"在数据库里更新 {recordsToUpdate.Count} 条数据");
        db.UpdateRange(recordsToUpdate);
        var updateRows = await db.SaveChangesAsync();
        logger.LogInformation("更新完成，已更新 {rows} 条数据", updateRows);

        return Result.Ok();
    }

    private VisitRecord InflateIpRegion(VisitRecord log) {
        if (string.IsNullOrWhiteSpace(log.Ip)) return log;

        var result = searcher.Search(log.Ip);
        if (string.IsNullOrWhiteSpace(result)) return log;

        var parts = result.Split('|');
        log.Country = parts[0];
        log.RegionCode = parts[1];
        log.Province = parts[2];
        log.City = parts[3];
        log.Isp = parts[4];

        return log;
    }
}