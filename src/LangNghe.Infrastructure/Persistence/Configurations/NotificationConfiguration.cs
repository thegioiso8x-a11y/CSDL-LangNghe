namespace LangNghe.Infrastructure.Persistence.Configurations;

using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Cấu hình ánh xạ bảng Sys_Notifications.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Sys_Notifications", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Message).HasMaxLength(1000);
        builder.Property(x => x.Type).IsRequired().HasMaxLength(50).IsUnicode(false);
        builder.Property(x => x.EntityType).HasMaxLength(100).IsUnicode(false);
        builder.Property(x => x.EntityId).HasMaxLength(50).IsUnicode(false);
        builder.Property(x => x.LinkUrl).HasMaxLength(500).IsUnicode(false);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Notification_User");

        builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt })
            .HasDatabaseName("IX_Notification_User_Read")
            .IsDescending(false, false, true);
    }
}
