using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarBlog.Data.Models;

namespace StarBlog.Data.Config;

public class VisitRecordConfig : IEntityTypeConfiguration<VisitRecord> {
    public void Configure(EntityTypeBuilder<VisitRecord> builder) {
        builder.ToTable("visit_record");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Ip).HasMaxLength(64);
        builder.Property(e => e.RequestPath).HasMaxLength(2048);
        builder.Property(e => e.RequestQueryString).HasMaxLength(2048);
        builder.Property(e => e.RequestMethod).HasMaxLength(10);
        builder.Property(e => e.UserAgent).HasMaxLength(1024);
    }
}