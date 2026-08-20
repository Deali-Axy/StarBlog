using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarBlog.Api.Modules.Links.Domain;

namespace StarBlog.Api.Modules.Links.Persistence;

/// <summary>友链申请表映射。</summary>
public sealed class LinkExchangeConfiguration : IEntityTypeConfiguration<LinkExchange> {
    public void Configure(EntityTypeBuilder<LinkExchange> builder) {
        builder.ToTable("link_exchange");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Name).HasMaxLength(128).IsRequired();
        builder.Property(item => item.Description).HasMaxLength(512);
        builder.Property(item => item.Url).HasMaxLength(512).IsRequired();
        builder.Property(item => item.WebMaster).HasMaxLength(128).IsRequired();
        builder.Property(item => item.Email).HasMaxLength(256).IsRequired();
        builder.Property(item => item.Reason).HasMaxLength(1024);
        builder.HasIndex(item => item.Url);
    }
}
