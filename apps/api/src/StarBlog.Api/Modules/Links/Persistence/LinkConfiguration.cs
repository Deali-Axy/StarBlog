using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarBlog.Api.Modules.Links.Domain;

namespace StarBlog.Api.Modules.Links.Persistence;

/// <summary>友链表映射。</summary>
public sealed class LinkConfiguration : IEntityTypeConfiguration<Link> {
    public void Configure(EntityTypeBuilder<Link> builder) {
        builder.ToTable("link");
        builder.HasKey(link => link.Id);
        builder.Property(link => link.Name).HasMaxLength(128).IsRequired();
        builder.Property(link => link.Description).HasMaxLength(512);
        builder.Property(link => link.Url).HasMaxLength(512).IsRequired();
    }
}
