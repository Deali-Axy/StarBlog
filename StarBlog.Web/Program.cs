using StarBlog.Contrib.SiteMessage;
using StarBlog.Data.Extensions;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
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

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();


app.UseSwagger();
app.UseSwaggerUI();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();