using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarBlog.Data.Extensions;
using StarBlog.Data.Models;

namespace StarBlog.Data.Config;

public class VisitRecordConfig : IEntityTypeConfiguration<VisitRecord> {
    public void Configure(EntityTypeBuilder<VisitRecord> builder) {
        builder.ToTable("visit_record");

        // 主键 + 自增
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Ip)
            .HasMaxLength(64);

        builder.OwnsOne(c => c.IpInfo, nb => {
            nb.Property(e => e.RegionCode)
                .HasMaxLength(128);
            nb.Property(e => e.Country)
                .HasMaxLength(128);
            nb.Property(e => e.Province)
                .HasMaxLength(128);
            nb.Property(e => e.City)
                .HasMaxLength(128);
            nb.Property(e => e.Isp)
                .HasMaxLength(128);
        });

        builder.Property(e => e.RequestPath)
            .HasMaxLength(2048);

        builder.Property(e => e.RequestQueryString)
            .HasMaxLength(2048);

        builder.Property(e => e.RequestMethod)
            .HasMaxLength(10);

        builder.Property(e => e.UserAgent)
            .HasMaxLength(1024);

        builder.OwnsOne(c => c.UserAgentInfo, info => {
            info.OwnsOne(e => e.OS, nb => {
                nb.Property(os => os.Family).HasMaxLength(50);
                nb.Property(os => os.Major).HasMaxLength(20);
                nb.Property(os => os.Minor).HasMaxLength(20);
                nb.Property(os => os.Patch).HasMaxLength(20);
                nb.Property(os => os.PatchMinor).HasMaxLength(20);
            });
            info.OwnsOne(e => e.Device, nb => {
                nb.Property(d => d.Family).HasMaxLength(50);
                nb.Property(d => d.Brand).HasMaxLength(50);
                nb.Property(d => d.Model).HasMaxLength(50);
            });
            info.OwnsOne(e => e.UserAgent, nb => {
                nb.Property(u => u.Family).HasMaxLength(50);
                nb.Property(u => u.Major).HasMaxLength(20);
                nb.Property(u => u.Minor).HasMaxLength(20);
                nb.Property(u => u.Patch).HasMaxLength(20);
            });
        });

        builder.Property(e => e.Time)
            .IsRequired();

        builder.Property(e => e.StatusCode);

        builder.Property(e => e.ResponseTimeMs);

        builder.Property(e => e.Referrer)
            .HasMaxLength(2048);

        // 索引
        builder.HasIndex(e => e.Time).HasDatabaseName("idx_visit_time");
        builder.HasIndex(e => e.RequestPath).HasDatabaseName("idx_visit_path");
        builder.HasIndex(e => e.StatusCode).HasDatabaseName("idx_visit_status");

        // builder.HasMultipleNestedOwnedIndexes(
        //     e => e.IpInfo, // 指定值对象 IpInfo
        //     new List<(Expression<Func<IpInfo, object>>, string)> {
        //         (ip => ip.RegionCode, "idx_visit_ip_info_region_code"),
        //         (ip => ip.Country, "idx_visit_ip_info_country"),
        //         (ip => ip.Province, "idx_visit_ip_info_province"),
        //         (ip => ip.City, "idx_visit_ip_info_city"),
        //         (ip => ip.Isp, "idx_visit_ip_info_isp")
        //     });


        // 复合索引，按日期+状态码查询
        builder.HasIndex(e => new { e.Time, e.StatusCode })
            .HasDatabaseName("idx_visit_time_status");
    }
}