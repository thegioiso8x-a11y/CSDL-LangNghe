using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class VillagePeriodicMetricConfiguration : IEntityTypeConfiguration<VillagePeriodicMetric>
{
    public void Configure(EntityTypeBuilder<VillagePeriodicMetric> builder)
    {
        builder.ToTable("Fact_Village_PeriodicMetrics", "dbo", t =>
        {
            t.HasCheckConstraint("CK_FactVillage_PeriodType", "PeriodType IN (1,2,3,4)");
            t.HasCheckConstraint("CK_FactVillage_ExportRate", "ExportRate BETWEEN 0 AND 100");
            t.HasCheckConstraint("CK_FactVillage_ImportRate", "ImportMaterialRate BETWEEN 0 AND 100");
            t.HasCheckConstraint("CK_FactVillage_ExportImport", "ExportRate + ImportMaterialRate <= 100");
            t.HasCheckConstraint("CK_FactVillage_ActiveMonths", "ActiveMonths IS NULL OR ActiveMonths BETWEEN 1 AND 12");
            t.HasCheckConstraint("CK_FactVillage_HouseholdCount", "HouseholdCount >= 0");
            t.HasCheckConstraint("CK_FactVillage_EnterpriseCount", "EnterpriseCount >= 0");
            t.HasCheckConstraint("CK_FactVillage_Revenue", "Revenue >= 0");
            t.HasCheckConstraint("CK_FactVillage_TotalLabor", "TotalLabor >= 0");
            t.HasCheckConstraint("CK_FactVillage_TrainedLaborRate", "TrainedLaborRate IS NULL OR TrainedLaborRate BETWEEN 0 AND 100");
        });

        // PK composite
        builder.HasKey(x => new { x.ReportPeriod, x.Id });
        builder.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityColumn();

        builder.Property(x => x.ReportPeriod).IsRequired().HasMaxLength(10).IsUnicode(false);
        builder.Property(x => x.ProductionProcess).HasMaxLength(500);
        builder.Property(x => x.TrainingNeed).HasMaxLength(1000);
        builder.Property(x => x.DataSource).HasMaxLength(50);

        builder.Property(x => x.ExportRate).HasColumnType("decimal(5,2)");
        builder.Property(x => x.ImportMaterialRate).HasColumnType("decimal(5,2)");
        builder.Property(x => x.ProductionArea).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Output).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Revenue).HasColumnType("decimal(18,2)");
        builder.Property(x => x.GrowthRate).HasColumnType("decimal(5,2)");
        builder.Property(x => x.TrainedLaborRate).HasColumnType("decimal(5,2)");
        builder.Property(x => x.AvgIncome).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DigitalLevel).HasColumnType("decimal(5,2)");
        builder.Property(x => x.SupportFunding).HasColumnType("decimal(18,2)");

        // Computed columns
        builder.Property(x => x.ExportValueVND)
            .HasColumnType("decimal(18,2)")
            .HasComputedColumnSql("(Revenue * ExportRate / 100)", stored: true);

        builder.Property(x => x.IsEffective)
            .HasComputedColumnSql("(CASE WHEN ActiveMonths >= 9 AND Revenue > 0 AND HasWasteCollection = 1 AND HasWasteWaterSystem = 1 AND HasSolidWasteSystem = 1 AND HasEmissionTreatment = 1 THEN 1 ELSE 0 END)", stored: true);

        builder.Property(x => x.ImportedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // Indexes
        builder.HasIndex(x => new { x.ReportPeriod, x.ImportBatchId }).HasDatabaseName("IX_FactVillage_Period_Import");
        builder.HasIndex(x => new { x.ReportPeriod, x.ProvinceId, x.CommuneId }).HasDatabaseName("IX_FactVillage_Period_Location");
        builder.HasIndex(x => new { x.CraftVillageId, x.ReportPeriod }).HasDatabaseName("IX_FactVillage_VillageId");

        // Relationships
        builder.HasOne(x => x.CraftVillage)
            .WithMany(x => x.VillagePeriodicMetrics)
            .HasForeignKey(x => x.CraftVillageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Commune)
            .WithMany()
            .HasForeignKey(x => x.CommuneId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_FactVillage_Commune");

        builder.HasOne(x => x.Province)
            .WithMany()
            .HasForeignKey(x => x.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_FactVillage_Province");

        builder.HasOne(x => x.ExportCategory)
            .WithMany()
            .HasForeignKey(x => x.ExportCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.OutputUnit)
            .WithMany()
            .HasForeignKey(x => x.OutputUnitId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
