using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class VillageTypeAggregateConfiguration : IEntityTypeConfiguration<VillageTypeAggregate>
{
    public void Configure(EntityTypeBuilder<VillageTypeAggregate> builder)
    {
        builder.ToTable("Agg_By_VillageType", "dbo");

        builder.HasKey(x => new { x.VillageTypeId, x.RegionId, x.Period, x.PeriodType });

        builder.Property(x => x.Period).IsRequired().HasMaxLength(10).IsUnicode(false);
        
        builder.Property(x => x.TotalRevenue).HasColumnType("decimal(18,2)");

        builder.Property(x => x.LastUpdated).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.VillageType)
            .WithMany(x => x.VillageTypeAggregates)
            .HasForeignKey(x => x.VillageTypeId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_VillageTypeAgg_VillageType");
    }
}
