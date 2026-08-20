using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarBlog.Api.Modules.Notifications.Domain;

namespace StarBlog.Api.Modules.Notifications.Persistence;

/// <summary>
/// Outbox 表映射。DedupKey 在非空时唯一，用于邮件去重。
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage> {
    public void Configure(EntityTypeBuilder<OutboxMessage> builder) {
        builder.ToTable("outbox_message");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Type).HasMaxLength(128).IsRequired();
        builder.Property(message => message.DedupKey).HasMaxLength(256);
        builder.Property(message => message.Payload).IsRequired();
        builder.Property(message => message.LastError).HasMaxLength(4000);
        builder.Property(message => message.LockedBy).HasMaxLength(256);
        builder.HasIndex(message => message.DedupKey)
            .IsUnique()
            .HasFilter("dedup_key IS NOT NULL");
        builder.HasIndex(message => new { message.Status, message.NextAttemptAt });
    }
}
