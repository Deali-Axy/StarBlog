using Ip2RegionDataProc.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ip2RegionDataProc.Data.Config;

public class AuditLogConfig : IEntityTypeConfiguration<AuditLog> {
    public void Configure(EntityTypeBuilder<AuditLog> builder) {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Username).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Timestamp).IsRequired();
        builder.Property(x => x.EntityName).HasMaxLength(256);
        builder.Property(x => x.EntityId).HasMaxLength(256);
    }
}