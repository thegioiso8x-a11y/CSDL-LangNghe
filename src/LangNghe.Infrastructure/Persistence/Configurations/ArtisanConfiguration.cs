using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class ArtisanConfiguration : IEntityTypeConfiguration<Artisan>
{
    public void Configure(EntityTypeBuilder<Artisan> builder)
    {
        builder.ToTable("Dim_Artisans", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CraftVillageId)
            .IsRequired();

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.BirthYear)
            .IsRequired(false);

        builder.Property(x => x.Gender)
            .IsRequired(false)
            .HasMaxLength(10);

        builder.Property(x => x.ArtisanLevel)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(x => x.RecognitionYear)
            .IsRequired(false);

        builder.Property(x => x.Specialty)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(x => x.Phone)
            .IsRequired(false)
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("DATETIME")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("DATETIME")
            .IsRequired(false);

        // Relationships
        builder.HasOne(x => x.CraftVillage)
            .WithMany(x => x.Artisans)
            .HasForeignKey(x => x.CraftVillageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => new { x.CraftVillageId, x.IsActive })
            .HasDatabaseName("IX_Artisans_Village")
            .IncludeProperties(x => new { x.FullName, x.ArtisanLevel });
    }
}