using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarBlog.Api.Modules.Analytics.Domain;

namespace StarBlog.Api.Modules.Analytics.Persistence;

/// <summary>
/// 访问记录映射，包含 IP / UA 值对象和查询索引。
/// </summary>
public sealed class VisitRecordConfiguration : IEntityTypeConfiguration<VisitRecord> {
    public void Configure(EntityTypeBuilder<VisitRecord> builder) {
        builder.ToTable("visit_record");
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Ip).HasMaxLength(64);
        builder.Property(record => record.RequestPath).HasMaxLength(2048).IsRequired();
        builder.Property(record => record.RequestQueryString).HasMaxLength(2048);
        builder.Property(record => record.RequestMethod).HasMaxLength(10).IsRequired();
        builder.Property(record => record.UserAgent).HasMaxLength(1024);
        builder.Property(record => record.Referrer).HasMaxLength(2048);
        builder.OwnsOne(record => record.IpInfo, owned => {
            owned.Property(info => info.RegionCode).HasMaxLength(128);
            owned.Property(info => info.Country).HasMaxLength(128);
            owned.Property(info => info.Province).HasMaxLength(128);
            owned.Property(info => info.City).HasMaxLength(128);
            owned.Property(info => info.Isp).HasMaxLength(128);
        });
        builder.OwnsOne(record => record.UserAgentInfo, info => {
            info.OwnsOne(value => value.OS, os => {
                os.Property(item => item.Family).HasMaxLength(50);
                os.Property(item => item.Major).HasMaxLength(20);
                os.Property(item => item.Minor).HasMaxLength(20);
                os.Property(item => item.Patch).HasMaxLength(20);
                os.Property(item => item.PatchMinor).HasMaxLength(20);
            });
            info.OwnsOne(value => value.Device, device => {
                device.Property(item => item.Family).HasMaxLength(50);
                device.Property(item => item.Brand).HasMaxLength(50);
                device.Property(item => item.Model).HasMaxLength(50);
            });
            info.OwnsOne(value => value.UserAgent, agent => {
                agent.Property(item => item.Family).HasMaxLength(50);
                agent.Property(item => item.Major).HasMaxLength(20);
                agent.Property(item => item.Minor).HasMaxLength(20);
                agent.Property(item => item.Patch).HasMaxLength(20);
            });
        });
        builder.HasIndex(record => record.Time).HasDatabaseName("idx_visit_time");
        builder.HasIndex(record => record.RequestPath).HasDatabaseName("idx_visit_path");
        builder.HasIndex(record => record.StatusCode).HasDatabaseName("idx_visit_status");
        builder.HasIndex(record => new { record.Time, record.StatusCode }).HasDatabaseName("idx_visit_time_status");
    }
}
