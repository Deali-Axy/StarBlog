using Microsoft.EntityFrameworkCore;
using StarBlog.Api.Modules.Analytics.Domain;
using StarBlog.Api.Modules.Comments.Domain;
using StarBlog.Api.Modules.Configuration.Domain;
using StarBlog.Api.Modules.Content.Domain;
using StarBlog.Api.Modules.Identity.Domain;
using StarBlog.Api.Modules.Links.Domain;
using StarBlog.Api.Modules.Media.Domain;
using StarBlog.Api.Modules.Notifications.Domain;

namespace StarBlog.Api.Infrastructure.Persistence;

/// <summary>
/// 整个模块化单体共享的 EF Core 上下文。实体映射由各模块的 IEntityTypeConfiguration 提供。
/// </summary>
public sealed class StarBlogDbContext : DbContext {
    public StarBlogDbContext(DbContextOptions<StarBlogDbContext> options) : base(options) {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ConfigItem> ConfigItems => Set<ConfigItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<Link> Links => Set<Link>();
    public DbSet<LinkExchange> LinkExchanges => Set<LinkExchange>();
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<FeaturedPhoto> FeaturedPhotos => Set<FeaturedPhoto>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<FeaturedPost> FeaturedPosts => Set<FeaturedPost>();
    public DbSet<FeaturedCategory> FeaturedCategories => Set<FeaturedCategory>();
    public DbSet<TopPost> TopPosts => Set<TopPost>();
    public DbSet<PostTranslation> PostTranslations => Set<PostTranslation>();
    public DbSet<PublicationChannel> PublicationChannels => Set<PublicationChannel>();
    public DbSet<PostPublication> PostPublications => Set<PostPublication>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<AnonymousUser> AnonymousUsers => Set<AnonymousUser>();
    public DbSet<VisitRecord> VisitRecords => Set<VisitRecord>();

    /// <summary>自动发现当前程序集中各模块的实体配置。</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StarBlogDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSnakeCaseNamingConvention();
    }
}
