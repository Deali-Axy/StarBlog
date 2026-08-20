using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarBlog.Api.Modules.Media.Domain;

namespace StarBlog.Api.Modules.Media.Persistence;

/// <summary>图片与推荐图片映射。</summary>
public sealed class PhotoConfiguration : IEntityTypeConfiguration<Photo> {
    public void Configure(EntityTypeBuilder<Photo> builder) {
        builder.ToTable("photo");
        builder.HasKey(photo => photo.Id);
        builder.Property(photo => photo.Id).HasMaxLength(32);
        builder.Property(photo => photo.Title).HasMaxLength(256).IsRequired();
        builder.Property(photo => photo.Location).HasMaxLength(256).IsRequired();
        builder.Property(photo => photo.FilePath).HasMaxLength(512).IsRequired();
    }
}

/// <summary>推荐图片映射，同一图片只能推荐一次。</summary>
public sealed class FeaturedPhotoConfiguration : IEntityTypeConfiguration<FeaturedPhoto> {
    public void Configure(EntityTypeBuilder<FeaturedPhoto> builder) {
        builder.ToTable("featured_photo");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.PhotoId).HasMaxLength(32).IsRequired();
        builder.HasIndex(item => item.PhotoId).IsUnique();
        builder.HasOne(item => item.Photo)
            .WithMany()
            .HasForeignKey(item => item.PhotoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
