using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Web.DependencyInjection;
using StarBlog.Application.Abstractions;
using StarBlog.Api.Filters;
using StarBlog.Api.Adapters;
using StarBlog.Api.Services.BackgroundTasks;
using StarBlog.Api.Services.OutboxServices;
using StarBlog.Application.Services;
using StarBlog.Application.Services.OutboxServices;
using StarBlog.Data;
using StarBlog.Data.Extensions;
using StarBlog.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 纯 WebAPI：仅注册 Controllers（不包含 MVC Views）
// 同时启用全局响应包装，确保绝大多数 Action 的返回结构稳定一致（ApiResponse / ApiResponsePaged）。
builder.Services.AddControllers(options => {
    options.Filters.Add<ResponseWrapperFilter>();
});

// 缓存与 HttpContextAccessor：部分服务（例如统计、IP/UA 解析、邮件/评论逻辑）会用到
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// 响应压缩：对 JSON/XML/静态文本等响应启用 Brotli/Gzip，减少带宽消耗
builder.Services.AddResponseCompression(options => {
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(new[] {
        "application/javascript",
        "application/json",
        "application/xml",
        "text/css",
        "text/html",
        "text/json",
        "text/plain",
        "text/xml"
    });
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options => {
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options => {
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

// AutoMapper：控制器 DTO <-> Entity 的映射
builder.Services.AddAutoMapper(typeof(Program));

// 访问日志/统计用的 EF Core SQLite（与 FreeSql 业务库并存）
builder.Services.AddDbContext<AppDbContext>(options => {
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLite-Log"));
});

// 业务数据访问（FreeSql）
builder.Services.AddFreeSql(builder.Configuration);
builder.Services.AddVisitRecord();
builder.Services.AddHttpClient();

// HealthChecks：提供 /health（汇总）、/health/live（存活）、/health/ready（就绪）端点
builder.Services.AddStarBlogHealthChecks();

// CORS：Next.js 前端跨域调用需要（带 Cookie/凭据时必须显式列出允许的 Origin）
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policyBuilder => {
        policyBuilder.AllowCredentials();
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyMethod();
        policyBuilder.WithOrigins(
            "http://localhost:3000",
            "http://localhost:5173",
            "http://localhost:8080",
            "http://localhost:8081",
            "https://deali.cn",
            "https://blog.deali.cn"
        );
    });
});

// Swagger：按既有分组输出 OpenAPI，并保留（可选）Swagger 访问授权中间件
builder.Services.AddSwagger();

// AppSettings：加载邮件/监控/安全相关配置（其中 email/monitoring 的 json 文件为 optional）
builder.Services.AddSettings(builder.Configuration);

// JWT Bearer：后台管理相关接口依赖
builder.Services.AddAuth(builder.Configuration);

// ImageSharp：保留现有图片处理能力（例如图片相关服务与中间件链路）
builder.Services.AddImageSharp();

builder.Services.AddSingleton<IAppPathProvider, AspNetAppPathProvider>();
builder.Services.AddSingleton<IFileStorage, PhysicalFileStorage>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

// 应用层服务：为了保证 API 行为与 StarBlog.Web 一致，首轮迁移直接沿用既有 Service 列表
builder.Services.AddSingleton<CommonService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddSingleton<TempFilterService>();
builder.Services.AddSingleton<MonitoringService>();
builder.Services.AddScoped<BlogService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<ConfigService>();
builder.Services.AddScoped<LinkExchangeService>();
builder.Services.AddScoped<LinkService>();
builder.Services.AddScoped<PhotoService>();
builder.Services.AddScoped<PostService>();
// 多平台发布：渠道及投递记录持久化在业务数据库，HTTP 客户端用于公众号官方 API。
builder.Services.AddScoped<PublicationService>();
// 文章翻译是可选能力：未配置密钥时 API 仍可启动，调用时给出可读错误。
builder.Services.Configure<TranslationConfig>(builder.Configuration.GetSection(TranslationConfig.SectionName));
builder.Services.AddScoped<TranslationService>();

// Outbox：将“需要后台处理的任务”（例如邮件发送）异步化，避免阻塞接口响应
builder.Services.Configure<OutboxOptions>(builder.Configuration.GetSection("Outbox"));
builder.Services.AddScoped<OutboxService>();
builder.Services.AddScoped<OutboxProcessor>();
builder.Services.AddScoped<IOutboxHandler, EmailSendOutboxHandler>();
builder.Services.AddHostedService<OutboxWorker>();
builder.Services.AddHostedService<BackgroundTaskWorker>();

// 上传/导入等场景可能包含大文件，放开 Kestrel 请求体大小限制（由业务逻辑自行校验）
builder.WebHost.ConfigureKestrel(options => {
    options.Limits.MaxRequestBodySize = long.MaxValue;
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}
else {
    app.UseExceptionHandler(applicationBuilder => {
        applicationBuilder.Run(async context => {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message = "Unexpected error!" });
        });
    });
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions {
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// 说明：ImageSharp 中间件需要存在 webroot（本项目提供空的 wwwroot 目录以满足启动）
app.UseImageSharp();
app.UseResponseCompression();

// 纯 API 不渲染 Razor，但文章图片、摄影原图等仍由 WebRoot 下的 media 目录承载。
app.UseStaticFiles();

// 访问分析服务已注册；将每个 HTTP 响应写入异步队列，供 /Api/Admin/Analytics 查询。
app.UseMiddleware<StarBlog.Api.Middlewares.VisitRecordMiddleware>();

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Swagger UI 可匿名打开，并在页面内通过 Authorize 输入 JWT；具体管理 API 仍受 [Authorize] 保护。
app.UseSwaggerPkg();

app.MapStarBlogHealthChecks();

app.MapControllers();

app.Run();

public partial class Program { }
