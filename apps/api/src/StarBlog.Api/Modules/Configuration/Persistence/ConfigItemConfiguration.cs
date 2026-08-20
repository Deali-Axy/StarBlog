using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarBlog.Api.Modules.Configuration.Domain;

namespace StarBlog.Api.Modules.Configuration.Persistence;

/// <summary>
/// 配置表映射：Key 唯一。
/// </summary>
public sealed class ConfigItemConfiguration : IEntityTypeConfiguration<ConfigItem> {
    public void Configure(EntityTypeBuilder<ConfigItem> builder) {
        builder.ToTable("config");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Key).HasMaxLength(128).IsRequired();
        builder.Property(item => item.Value).HasMaxLength(2048).IsRequired();
        builder.Property(item => item.Description).HasMaxLength(512);
        builder.HasIndex(item => item.Key).IsUnique();
    }
}
