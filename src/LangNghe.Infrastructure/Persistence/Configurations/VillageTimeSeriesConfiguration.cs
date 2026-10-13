using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class VillageTimeSeriesConfiguration : IEntityTypeConfiguration<VillageTimeSeries>
{
    public void Configure(EntityTypeBuilder<VillageTimeSeries> builder)
    {
        builder.ToTable("Agg_Village_TimeSeries", "dbo");

        builder.HasKey(x => new { x.RegionId, x.Period, x.PeriodType });

        builder.Property(x => x.Period).IsRequired().HasMaxLength(10).IsUnicode(false);
        builder.Property(x => x.CalculatedBy).HasMaxLength(200);
        
        builder.Property(x => x.TotalArea).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalRevenue).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalExportValue).HasColumnType("decimal(18,2)");
        builder.Property(x => x.AvgRevenuePerVillage).HasColumnType("decimal(18,2)");
        builder.Property(x => x.AvgIncome).HasColumnType("decimal(18,2)");
        builder.Property(x => x.AvgGrowthRate).HasColumnType("decimal(5,2)");

        builder.Property(x => x.LastUpdated).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.Region)
            .WithMany(x => x.VillageTimeSeries)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_VillageTimeSeries_Region");
    }
}
