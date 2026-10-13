using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class VillageEnvironmentConfiguration : IEntityTypeConfiguration<VillageEnvironment>
{
    public void Configure(EntityTypeBuilder<VillageEnvironment> builder)
    {
        builder.ToTable("Fact_Village_Environment", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SurveyPeriod).IsRequired().HasMaxLength(10).IsUnicode(false);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        
        builder.Property(x => x.ImportedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // UQ
        builder.HasIndex(x => new { x.CraftVillageId, x.SurveyPeriod })
            .IsUnique()
            .HasDatabaseName("UQ_VillageEnvironment_Period");

        // Relationships
        builder.HasOne(x => x.CraftVillage)
            .WithMany(x => x.VillageEnvironments)
            .HasForeignKey(x => x.CraftVillageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
