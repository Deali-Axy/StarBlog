﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

        builder.Property(e => e.RequestPath)
            .HasMaxLength(2048);

        builder.Property(e => e.RequestQueryString)
            .HasMaxLength(2048);

        builder.Property(e => e.RequestMethod)
            .HasMaxLength(10);

        builder.Property(e => e.UserAgent)
            .HasMaxLength(1024);

        builder.Property(e => e.Time)
            .IsRequired();

        builder.Property(e => e.StatusCode);

        builder.Property(e => e.ResponseTimeMs);

        builder.Property(e => e.Referrer)
            .HasMaxLength(2048);

        builder.Property(e => e.RegionCode)
            .HasMaxLength(128);
        builder.Property(e => e.Country)
            .HasMaxLength(128);
        builder.Property(e => e.Province)
            .HasMaxLength(128);
        builder.Property(e => e.City)
            .HasMaxLength(128);
        builder.Property(e => e.Isp)
            .HasMaxLength(128);

        // 索引
        builder.HasIndex(e => e.RequestPath).HasDatabaseName("idx_visit_url");
        builder.HasIndex(e => e.Time).HasDatabaseName("idx_visit_time");
        builder.HasIndex(e => e.RequestPath).HasDatabaseName("idx_visit_path");
        builder.HasIndex(e => e.StatusCode).HasDatabaseName("idx_visit_status");
        builder.HasIndex(e => e.RegionCode).HasDatabaseName("idx_visit_region_code");
        builder.HasIndex(e => e.Country).HasDatabaseName("idx_visit_country");
        builder.HasIndex(e => e.Province).HasDatabaseName("idx_visit_province");
        builder.HasIndex(e => e.City).HasDatabaseName("idx_visit_city");
        builder.HasIndex(e => e.Isp).HasDatabaseName("idx_visit_isp");

        // 复合索引，按日期+状态码查询
        builder.HasIndex(e => new { e.Time, e.StatusCode })
            .HasDatabaseName("idx_visit_time_status");
    }
}