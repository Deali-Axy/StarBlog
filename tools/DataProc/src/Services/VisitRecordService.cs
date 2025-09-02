using System.IO;
using System.Text.Json;
using AutoMapper;
using DataProc.Entities;
using Dumpify;
using DataProc.Utilities;
using FluentResults;
using IP2Region.Net.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarBlog.Data;
using StarBlog.Data.Models;
using UAParser;
using OS = StarBlog.Data.Models.OS;

namespace DataProc.Services;

public class VisitRecordService(
    ILogger<VisitRecordService> logger,
    IOptions<AppSettings> options,
    IConfiguration conf,
    ISearcher searcher,
    IMapper mapper,
    AppDbContext db)
    : IService {
    private readonly AppSettings _settings = options.Value;
    private readonly IConfiguration _conf = conf;
    private readonly Parser uaParser = Parser.GetDefault();

    public async Task<Result> Run() {
        logger.LogInformation("启动！");

        var records = await db.VisitRecords.ToListAsync();
        logger.LogInformation($"已读取 {records.Count} 条访问日志，正在处理IP地址");
        var recordsToUpdate = records.Select(InflateIpRegion).ToList();
        logger.LogInformation("正在处理 UserAgent 信息");
        recordsToUpdate = recordsToUpdate.Select(InflateUA).ToList();
        logger.LogInformation($"在数据库里更新 {recordsToUpdate.Count} 条数据");
        db.UpdateRange(recordsToUpdate);
        var updateRows = await db.SaveChangesAsync();
        logger.LogInformation("更新完成，已更新 {rows} 条数据", updateRows);

        InflateUA(records.First());

        return Result.Ok();
    }

    private VisitRecord InflateIpRegion(VisitRecord log) {
        if (string.IsNullOrWhiteSpace(log.Ip)) return log;

        var result = searcher.Search(log.Ip);
        if (string.IsNullOrWhiteSpace(result)) return log;

        var parts = result.Split('|');
        log.IpInfo = new IpInfo {
            Country = parts[0],
            RegionCode = parts[1],
            Province = parts[2],
            City = parts[3],
            Isp = parts[4]
        };

        return log;
    }

    private VisitRecord InflateUA(VisitRecord log) {
        var c = uaParser.Parse(log.UserAgent);
        log.UserAgentInfo = mapper.Map<UserAgentInfo>(c);
        if (!string.IsNullOrWhiteSpace(log.UserAgentInfo.UserAgent.Family)
            && log.UserAgentInfo.UserAgent.Family.ToLower().Contains("bytespider")) {
            log.UserAgentInfo.Device.IsSpider = true;
        }

        return log;
    }
}