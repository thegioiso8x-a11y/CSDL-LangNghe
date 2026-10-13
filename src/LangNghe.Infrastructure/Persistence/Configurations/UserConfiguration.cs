namespace LangNghe.Infrastructure.Persistence.Configurations;

using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Sys_Users", "dbo", t =>
        {
            t.HasCheckConstraint("CK_Sys_Users_DataScope", "DataScope IN (1,2,3)");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.FullName)
            .HasMaxLength(255);

        builder.Property(x => x.Email)
            .HasMaxLength(255);

        builder.Property(x => x.Phone)
            .HasMaxLength(20);

        builder.Property(x => x.LastLoginIp)
            .HasMaxLength(50);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.FailedLoginCount)
            .HasDefaultValue(0);

        builder.Property(x => x.MustChangePassword)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Indexes
        builder.HasIndex(x => x.Username)
            .IsUnique()
            .HasDatabaseName("UQ_Sys_Users_Username");

        builder.HasIndex(x => new { x.RegionId, x.DataScope, x.IsActive })
            .HasDatabaseName("IX_Users_Region")
            .IncludeProperties(x => new { x.Username, x.FullName, x.Email });

        // Relationships
        builder.HasOne(x => x.Region)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
