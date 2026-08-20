using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarBlog.Api.Modules.Identity.Domain;

namespace StarBlog.Api.Modules.Identity.Persistence;

/// <summary>
/// 用户表映射：用户名唯一。
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User> {
    public void Configure(EntityTypeBuilder<User> builder) {
        builder.ToTable("user");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).HasMaxLength(32);
        builder.Property(user => user.Name).HasMaxLength(64).IsRequired();
        builder.Property(user => user.Password).HasMaxLength(512).IsRequired();
        builder.HasIndex(user => user.Name).IsUnique();
    }
}
