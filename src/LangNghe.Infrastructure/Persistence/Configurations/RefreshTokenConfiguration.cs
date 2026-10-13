using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("Sys_RefreshTokens", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(128)
            .IsUnicode(false);

        builder.Property(x => x.RevokedByIp)
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(x => x.CreatedByIp)
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(x => x.ReplacedBy)
            .HasMaxLength(128)
            .IsUnicode(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(x => x.Token, "UQ_Sys_RefreshTokens_Token")
            .IsUnique();

        builder.HasIndex(x => new { x.UserId, x.ExpiresAt }, "IX_RefreshToken_User")
            .IsDescending(false, true)
            .IncludeProperties(x => x.RevokedAt);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_RefreshToken_User");
    }
}
