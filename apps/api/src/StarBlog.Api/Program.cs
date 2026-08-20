using Microsoft.EntityFrameworkCore;
using StarBlog.Api.Hosting;
using StarBlog.Api.Hosting.Middleware;
using StarBlog.Api.Infrastructure;
using StarBlog.Api.Infrastructure.Persistence;
using StarBlog.Api.Modules.Analytics;
using StarBlog.Api.Modules.Comments;
using StarBlog.Api.Modules.Configuration;
using StarBlog.Api.Modules.Content;
using StarBlog.Api.Modules.Identity;
using StarBlog.Api.Modules.Links;
using StarBlog.Api.Modules.Media;
using StarBlog.Api.Modules.Notifications;
using StarBlog.Api.Modules.Site;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings-email.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings-monitoring.json", optional: true, reloadOnChange: true);

builder.Services.AddWebFramework(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddIdentityModule();
builder.Services.AddConfigurationModule();
builder.Services.AddNotificationsModule(builder.Configuration);
builder.Services.AddLinksModule();
builder.Services.AddMediaModule();
builder.Services.AddContentModule(builder.Configuration);
builder.Services.AddCommentsModule();
builder.Services.AddAnalyticsModule();
builder.Services.AddSiteModule();

builder.WebHost.ConfigureKestrel(options => {
    options.Limits.MaxRequestBodySize = long.MaxValue;
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope()) {
    var db = scope.ServiceProvider.GetRequiredService<StarBlogDbContext>();
    await db.Database.MigrateAsync();

    await scope.ServiceProvider.GetRequiredService<ContentOperations>().EnsureDefaultCategoryAsync(CancellationToken.None);
}

app.UseWebFramework();
app.UseMiddleware<VisitRecordMiddleware>();

app.MapIdentityModule();
app.MapConfigurationModule();
app.MapNotificationsModule();
app.MapLinksModule();
app.MapMediaModule();
app.MapContentModule();
app.MapCommentsModule();
app.MapAnalyticsModule();
app.MapSiteModule();

app.Run();

public partial class Program;
