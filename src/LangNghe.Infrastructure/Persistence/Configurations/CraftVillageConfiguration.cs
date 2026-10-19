using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class CraftVillageConfiguration : IEntityTypeConfiguration<CraftVillage>
{
    public void Configure(EntityTypeBuilder<CraftVillage> builder)
    {
        builder.ToTable("Dim_CraftVillages", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.VillageCode)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.RegionId)
            .IsRequired();

        builder.Property(x => x.Address)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Latitude)
            .HasColumnType("decimal(10,7)");

        builder.Property(x => x.Longitude)
            .HasColumnType("decimal(10,7)");

        builder.Property(x => x.GeocodeSource)
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(x => x.RecognitionDecision)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(x => x.RecognitionDate)
            .IsRequired(false)
            .HasColumnType("DATE");

        builder.Property(x => x.ClassificationId)
            .IsRequired(false);

        builder.Property(x => x.VillageTypeId)
            .IsRequired(false);

        builder.Property(x => x.IndustryGroupId)
            .IsRequired(false);

        builder.Property(x => x.NeedRestoration)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.IsPublished)
            .IsRequired();

        builder.Property(x => x.PublishedAt)
            .HasColumnType("DATETIME")
            .IsRequired(false);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("DATETIME")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(x => x.CreatedBy)
            .IsRequired(false);

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("DATETIME")
            .IsRequired(false);

        builder.Property(x => x.UpdatedBy)
            .IsRequired(false);

        // Relationships
        builder.HasOne(x => x.Region)
            .WithMany(x => x.CraftVillages)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Classification)
            .WithMany(x => x.CraftVillages)
            .HasForeignKey(x => x.ClassificationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.VillageType)
            .WithMany(x => x.CraftVillages)
            .HasForeignKey(x => x.VillageTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.IndustryGroup)
            .WithMany(x => x.CraftVillages)
            .HasForeignKey(x => x.IndustryGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => new { x.RegionId, x.Status })
            .HasDatabaseName("IX_Villages_Region")
            .IncludeProperties(x => new { x.VillageCode, x.Name, x.VillageTypeId, x.IndustryGroupId });

        builder.HasIndex(x => new { x.VillageTypeId, x.Status })
            .HasDatabaseName("IX_Villages_Type")
            .IncludeProperties(x => new { x.Name, x.RegionId });

        builder.HasIndex(x => x.VillageCode)
            .IsUnique()
            .HasDatabaseName("UQ_Dim_CraftVillages_Code");

        builder.HasIndex(x => new { x.Latitude, x.Longitude })
            .HasDatabaseName("IX_Village_Geo");

        // Checks
        builder.ToTable(t => 
        {
            t.HasCheckConstraint("CK_Villages_Code", "VillageCode LIKE '[0-9][0-9][0-9][0-9][0-9]'");
            t.HasCheckConstraint("CK_Villages_Status", "Status IN (0,1,2)");
        });
    }
}
