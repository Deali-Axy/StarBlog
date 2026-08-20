using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarBlog.Api.Modules.Comments.Domain;

namespace StarBlog.Api.Modules.Comments.Persistence;

/// <summary>评论映射。不建立对 Post 的外键，跨模块只通过 PostId 引用。</summary>
public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment> {
    public void Configure(EntityTypeBuilder<Comment> builder) {
        builder.ToTable("comment");
        builder.HasKey(comment => comment.Id);
        builder.Property(comment => comment.Id).HasMaxLength(32);
        builder.Property(comment => comment.PostId).HasMaxLength(32).IsRequired();
        builder.Property(comment => comment.Content).IsRequired();
        builder.HasIndex(comment => comment.PostId);
        builder.HasOne(comment => comment.Parent)
            .WithMany(comment => comment.Replies)
            .HasForeignKey(comment => comment.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(comment => comment.AnonymousUser)
            .WithMany()
            .HasForeignKey(comment => comment.AnonymousUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>匿名用户映射，邮箱唯一。</summary>
public sealed class AnonymousUserConfiguration : IEntityTypeConfiguration<AnonymousUser> {
    public void Configure(EntityTypeBuilder<AnonymousUser> builder) {
        builder.ToTable("anonymous_user");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).HasMaxLength(32);
        builder.Property(user => user.Name).HasMaxLength(128).IsRequired();
        builder.Property(user => user.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();
    }
}
