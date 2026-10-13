using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("Dim_Regions", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RegionCode)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ShortName)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(x => x.LevelType)
            .IsRequired();

        builder.Property(x => x.RegionType)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.ParentId)
            .IsRequired(false);

        builder.Property(x => x.MergedFromCode)
            .IsRequired(false)
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(x => x.SortOrder)
            .IsRequired(false);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("DATETIME")
            .HasDefaultValueSql("SYSUTCDATETIME()");
            
        builder.Property(x => x.UpdatedAt)
            .HasColumnType("DATETIME")
            .IsRequired(false);

        // Relationships
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Agg_Village_TimeSeries có RegionId = 0 cho Toàn quốc, không có
        // bản ghi tương ứng trong Dim_Regions nên không tạo quan hệ FK.
        builder.Ignore(x => x.VillageTimeSeries);

        // Indexes
        builder.HasIndex(x => x.RegionCode)
            .IsUnique()
            .HasDatabaseName("UQ_Dim_Regions_Code");

        builder.HasIndex(x => new { x.ParentId, x.LevelType, x.IsActive })
            .HasDatabaseName("IX_Regions_Parent")
            .IncludeProperties(x => new { x.RegionCode, x.Name });

        // Checks
        builder.ToTable(t => t.HasCheckConstraint("CK_Dim_Regions_Level", "LevelType IN (1,2)"));
    }
}
