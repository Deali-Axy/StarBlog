using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarBlog.Api.Modules.Content.Domain;

namespace StarBlog.Api.Modules.Content.Persistence;

/// <summary>分类映射。ParentId 为 0 表示根分类，不建立数据库外键以免根节点无法插入。</summary>
public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category> {
    public void Configure(EntityTypeBuilder<Category> builder) {
        builder.ToTable("category");
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Name).HasMaxLength(128).IsRequired();
        builder.HasMany(category => category.Posts)
            .WithOne(post => post.Category)
            .HasForeignKey(post => post.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>文章映射。Slug 在非空时唯一。</summary>
public sealed class PostConfiguration : IEntityTypeConfiguration<Post> {
    public void Configure(EntityTypeBuilder<Post> builder) {
        builder.ToTable("post");
        builder.HasKey(post => post.Id);
        builder.Property(post => post.Id).HasMaxLength(32);
        builder.Property(post => post.Title).HasMaxLength(256).IsRequired();
        builder.Property(post => post.Slug).HasMaxLength(150);
        builder.Property(post => post.Status).HasMaxLength(64);
        builder.Property(post => post.Content);
        builder.Property(post => post.Path).HasMaxLength(512);
        builder.Property(post => post.Categories).HasMaxLength(256);
        builder.HasIndex(post => post.Slug).IsUnique().HasFilter("slug IS NOT NULL");
        builder.HasIndex(post => post.IsPublish);
    }
}

/// <summary>推荐文章映射。</summary>
public sealed class FeaturedPostConfiguration : IEntityTypeConfiguration<FeaturedPost> {
    public void Configure(EntityTypeBuilder<FeaturedPost> builder) {
        builder.ToTable("featured_post");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.PostId).HasMaxLength(32).IsRequired();
        builder.HasIndex(item => item.PostId).IsUnique();
        builder.HasOne(item => item.Post).WithMany().HasForeignKey(item => item.PostId).OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>置顶文章映射。</summary>
public sealed class TopPostConfiguration : IEntityTypeConfiguration<TopPost> {
    public void Configure(EntityTypeBuilder<TopPost> builder) {
        builder.ToTable("top_post");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.PostId).HasMaxLength(32).IsRequired();
        builder.HasIndex(item => item.PostId).IsUnique();
        builder.HasOne(item => item.Post).WithMany().HasForeignKey(item => item.PostId).OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>推荐分类映射。</summary>
public sealed class FeaturedCategoryConfiguration : IEntityTypeConfiguration<FeaturedCategory> {
    public void Configure(EntityTypeBuilder<FeaturedCategory> builder) {
        builder.ToTable("featured_category");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Name).HasMaxLength(128).IsRequired();
        builder.Property(item => item.Description).HasMaxLength(512).IsRequired();
        builder.Property(item => item.IconCssClass).HasMaxLength(128).IsRequired();
        builder.HasIndex(item => item.CategoryId).IsUnique();
        builder.HasOne(item => item.Category).WithMany().HasForeignKey(item => item.CategoryId).OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>译本映射，同一文章同一语言唯一。</summary>
public sealed class PostTranslationConfiguration : IEntityTypeConfiguration<PostTranslation> {
    public void Configure(EntityTypeBuilder<PostTranslation> builder) {
        builder.ToTable("post_translation");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasMaxLength(36);
        builder.Property(item => item.PostId).HasMaxLength(32).IsRequired();
        builder.Property(item => item.Language).HasMaxLength(16).IsRequired();
        builder.Property(item => item.Title).HasMaxLength(256).IsRequired();
        builder.HasIndex(item => new { item.PostId, item.Language }).IsUnique();
        builder.HasOne(item => item.Post)
            .WithMany(post => post.Translations)
            .HasForeignKey(item => item.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>发布渠道映射。</summary>
public sealed class PublicationChannelConfiguration : IEntityTypeConfiguration<PublicationChannel> {
    public void Configure(EntityTypeBuilder<PublicationChannel> builder) {
        builder.ToTable("publication_channel");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasMaxLength(32);
        builder.Property(item => item.Name).HasMaxLength(80).IsRequired();
        builder.Property(item => item.AppSecret).HasMaxLength(512);
    }
}

/// <summary>发布记录映射。</summary>
public sealed class PostPublicationConfiguration : IEntityTypeConfiguration<PostPublication> {
    public void Configure(EntityTypeBuilder<PostPublication> builder) {
        builder.ToTable("post_publication");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasMaxLength(32);
        builder.Property(item => item.PostId).HasMaxLength(32).IsRequired();
        builder.Property(item => item.ChannelId).HasMaxLength(32).IsRequired();
        builder.Property(item => item.Title).HasMaxLength(256).IsRequired();
        builder.Property(item => item.RenderedContent).IsRequired();
        builder.HasIndex(item => item.PostId);
    }
}
