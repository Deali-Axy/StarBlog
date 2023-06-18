using Microsoft.AspNetCore.HttpOverrides;
using RobotsTxt;
using SixLabors.ImageSharp.Web.DependencyInjection;
using StarBlog.Data.Extensions;
using StarBlog.Web.Contrib.SiteMessage;
using StarBlog.Web.Extensions;
using StarBlog.Web.Filters;
using StarBlog.Web.Middlewares;
using StarBlog.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var mvcBuilder = builder.Services.AddControllersWithViews(
    options => { options.Filters.Add<ResponseWrapperFilter>(); }
);
// 开发模式启用Razor页面动态编译
if (builder.Environment.IsDevelopment()) {
    mvcBuilder.AddRazorRuntimeCompilation();
}

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddFreeSql(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policyBuilder => {
        policyBuilder.AllowCredentials();
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyMethod();
        // policyBuilder.AllowAnyOrigin();
        policyBuilder.WithOrigins("http://localhost:8080");
        policyBuilder.WithOrigins("https://deali.cn");
    });
});
builder.Services.AddStaticRobotsTxt(builder => builder
    .AddSection(section => section.AddUserAgent("Googlebot").Allow("/"))
    .AddSection(section => section.AddUserAgent("bingbot").Allow("/"))
    .AddSection(section => section.AddUserAgent("Bytespider").Allow("/"))
    .AddSection(section => section.AddUserAgent("Sogou web spider").Allow("/"))
    .AddSection(section => section.AddUserAgent("*").Disallow("/"))
);
builder.Services.AddSwagger();
builder.Services.AddSettings(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
// 注册 IHttpClientFactory，参考：https://docs.microsoft.com/zh-cn/dotnet/core/extensions/http-client
builder.Services.AddHttpClient();
builder.Services.AddImageSharp();
// 注册自定义服务
builder.Services.AddSingleton<CommonService>();
builder.Services.AddSingleton<CrawlService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<MessageService>();
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddSingleton<PicLibService>();
builder.Services.AddSingleton<TempFilterService>();
builder.Services.AddScoped<BlogService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<ConfigService>();
builder.Services.AddScoped<LinkExchangeService>();
builder.Services.AddScoped<LinkService>();
builder.Services.AddScoped<PhotoService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<VisitRecordService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions {
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseImageSharp();
// app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions {
    ServeUnknownFileTypes = true
});

// app.UseMiddleware<VisitRecordMiddleware>();
app.UseRobotsTxt();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.UseSwaggerPkg();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();