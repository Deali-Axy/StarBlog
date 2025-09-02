using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using RobotsTxt;
using SixLabors.ImageSharp.Web.DependencyInjection;
using StarBlog.Data;
using StarBlog.Data.Extensions;
using StarBlog.Web.Contrib.SiteMessage;
using StarBlog.Web.Extensions;
using StarBlog.Web.Filters;
using StarBlog.Web.Middlewares;
using StarBlog.Web.Services;

var builder = WebApplication.CreateBuilder(args);


var mvcBuilder = builder.Services.AddControllersWithViews(options => { options.Filters.Add<ResponseWrapperFilter>(); }
);

// 开发模式启用Razor页面动态编译
if (builder.Environment.IsDevelopment()) {
    mvcBuilder.AddRazorRuntimeCompilation();
}

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// 添加响应压缩
builder.Services.AddResponseCompression(options => {
    options.EnableForHttps = true;

    // 配置压缩提供程序（按优先级排序）
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();

    // 配置要压缩的 MIME 类型
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

// 配置 Brotli 压缩级别
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options => {
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

// 配置 Gzip 压缩级别
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options => {
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddDbContext<AppDbContext>(options => {
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLite-Log"));
});
builder.Services.AddFreeSql(builder.Configuration);
builder.Services.AddVisitRecord();
builder.Services.AddHttpClient();
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policyBuilder => {
        policyBuilder.AllowCredentials();
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyMethod();
        // policyBuilder.AllowAnyOrigin();
        policyBuilder.WithOrigins("http://localhost:3000");
        policyBuilder.WithOrigins("http://localhost:8080");
        policyBuilder.WithOrigins("http://localhost:8081");
        policyBuilder.WithOrigins("https://deali.cn");
        policyBuilder.WithOrigins("https://blog.deali.cn");
    });
});
builder.Services.AddStaticRobotsTxt(opt => {
    var baseUrl = builder.Configuration["host"] ?? "https://blog.deali.cn";
    opt.AddSection(section => section.AddUserAgent("Googlebot").Allow("/"))
        .AddSection(section => section.AddUserAgent("bingbot").Allow("/"))
        .AddSection(section => section.AddUserAgent("Bytespider").Disallow("/"))
        .AddSection(section => section.AddUserAgent("Sogou web spider").Allow("/"))
        .AddSection(section => section.AddUserAgent("*")
            .Disallow("/Admin/")
            .Disallow("/Api/")
            .Disallow("/bin/")
            .Disallow("/obj/")
            .Disallow("/node_modules/")
            .Disallow("/seo-test")
            .Allow("/"))
        .AddSitemap($"{baseUrl}/sitemap.xml")
        .AddSitemap($"{baseUrl}/sitemap-images.xml");

    return opt;
});
builder.Services.AddSwagger();
builder.Services.AddSettings(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
// 注册 IHttpClientFactory，参考：https://docs.microsoft.com/zh-cn/dotnet/core/extensions/http-client
builder.Services.AddHttpClient();
builder.Services.AddImageSharp();
// 注册自定义服务
builder.Services.AddSingleton<CommonService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<MessageService>();
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
builder.Services.AddScoped<SeoService>();
builder.Services.AddScoped<StructuredDataService>();
builder.Services.AddScoped<ImageSeoService>();
builder.Services.AddScoped<SitemapService>();

// 设置请求最大大小
builder.WebHost.ConfigureKestrel(options => { options.Limits.MaxRequestBodySize = long.MaxValue; });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}
else {
    app.UseExceptionHandler(applicationBuilder => {
        applicationBuilder.Run(async context => {
            // todo 记录错误日志
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message = "Unexpected error!" });
        });
    });
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions {
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseImageSharp();
// app.UseHttpsRedirection();

// 启用响应压缩
app.UseResponseCompression();

// 添加静态文件调试中间件（仅在非生产环境）
if (!app.Environment.IsProduction()) {
    app.UseMiddleware<StaticFileDebugMiddleware>();
}

// 配置静态文件缓存
app.UseStaticFiles(new StaticFileOptions {
    ServeUnknownFileTypes = true,
    OnPrepareResponse = ctx => {
        var path = ctx.Context.Request.Path.Value;

        // 对于 JS 和 CSS 文件，设置较短的缓存时间以便调试
        if (path != null && (path.EndsWith(".js") || path.EndsWith(".css"))) {
            if (app.Environment.IsDevelopment()) {
                // 开发环境不缓存 JS/CSS
                ctx.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                ctx.Context.Response.Headers.Pragma = "no-cache";
                ctx.Context.Response.Headers.Expires = "0";
            }
            else {
                // 生产环境短期缓存 JS/CSS
                const int shortDuration = 60 * 60 * 24; // 1天
                ctx.Context.Response.Headers.CacheControl = $"public,max-age={shortDuration}";
                ctx.Context.Response.Headers.Expires = DateTime.UtcNow.AddDays(1).ToString("R");
            }
        }
        else {
            // 其他静态文件长期缓存
            const int durationInSeconds = 60 * 60 * 24 * 30; // 30天
            ctx.Context.Response.Headers.CacheControl = $"public,max-age={durationInSeconds}";
            ctx.Context.Response.Headers.Expires = DateTime.UtcNow.AddDays(30).ToString("R");
        }
    }
});

app.UseMiddleware<VisitRecordMiddleware>();
app.UseRobotsTxt();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.UseSwaggerPkg();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();