using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class IndustryGroupAggregateConfiguration : IEntityTypeConfiguration<IndustryGroupAggregate>
{
    public void Configure(EntityTypeBuilder<IndustryGroupAggregate> builder)
    {
        builder.ToTable("Agg_By_IndustryGroup", "dbo");

        builder.HasKey(x => new { x.IndustryGroupId, x.RegionId, x.Period, x.PeriodType });

        builder.Property(x => x.Period).IsRequired().HasMaxLength(10).IsUnicode(false);
        
        builder.Property(x => x.TotalRevenue).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalExportValue).HasColumnType("decimal(18,2)");

        builder.Property(x => x.LastUpdated).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.IndustryGroup)
            .WithMany(x => x.IndustryGroupAggregates)
            .HasForeignKey(x => x.IndustryGroupId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_IndustryGroupAgg_IndustryGroup");
    }
}
