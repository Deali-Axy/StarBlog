using Microsoft.AspNetCore.Diagnostics;
using StarBlog.Contrib.SiteMessage;
using StarBlog.Data.Extensions;
using StarBlog.Web.Extensions;
using StarBlog.Web.Filters;
using StarBlog.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options => { options.Filters.Add<ResponseWrapperFilter>(); });
builder.Services.AddFreeSql(builder.Configuration);
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policyBuilder => {
        policyBuilder.AllowCredentials();
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyMethod();
        // policyBuilder.AllowAnyOrigin();
        policyBuilder.WithOrigins("http://localhost:8080");
    });
});
builder.Services.AddSwagger();
builder.Services.AddSettings(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);

// 自定义服务
builder.Services.AddScoped<BlogService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<PhotoService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddSingleton<Messages>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseExceptionHandler(exceptionHandlerApp => {
    exceptionHandlerApp.Run(async context => {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        context.Response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain;

        await context.Response.WriteAsync("An exception was thrown.");

        var exceptionHandlerPathFeature =
            context.Features.Get<IExceptionHandlerPathFeature>();

        if (exceptionHandlerPathFeature?.Error is FileNotFoundException) {
            await context.Response.WriteAsync(" The file was not found.");
        }

        if (exceptionHandlerPathFeature?.Path == "/") {
            await context.Response.WriteAsync(" Page: Home.");
        }
    });
});

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();


app.UseSwagger();
app.UseSwaggerUI(options => {
    options.RoutePrefix = "api-docs/swagger";
    options.SwaggerEndpoint("/swagger/blog/swagger.json", "Blog");
    options.SwaggerEndpoint("/swagger/auth/swagger.json", "Auth");
    options.SwaggerEndpoint("/swagger/common/swagger.json", "Common");
    options.SwaggerEndpoint("/swagger/test/swagger.json", "Test");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();