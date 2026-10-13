using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class RegionMappingConfiguration : IEntityTypeConfiguration<RegionMapping>
{
    public void Configure(EntityTypeBuilder<RegionMapping> builder)
    {
        builder.ToTable("Dim_RegionMapping", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OldCode)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(x => x.NewCode)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(x => x.OldName)
            .IsRequired(false)
            .HasMaxLength(255);

        builder.Property(x => x.NewName)
            .IsRequired(false)
            .HasMaxLength(255);

        builder.Property(x => x.LevelType)
            .IsRequired();

        builder.Property(x => x.EffectiveDate)
            .IsRequired()
            .HasColumnType("DATE");

        builder.Property(x => x.DecreeNumber)
            .IsRequired(false)
            .HasMaxLength(200);

        builder.Property(x => x.Notes)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("DATETIME")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Indexes
        builder.HasIndex(x => new { x.OldCode, x.NewCode, x.EffectiveDate })
            .IsUnique()
            .HasDatabaseName("UQ_RegionMapping");
    }
}