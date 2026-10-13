using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class ProductOcopHistoryConfiguration : IEntityTypeConfiguration<ProductOcopHistory>
{
    public void Configure(EntityTypeBuilder<ProductOcopHistory> builder)
    {
        builder.ToTable("Fact_Product_OCOP_History", "dbo", t =>
        {
            t.HasCheckConstraint("CK_OcopStar", "OcopStar BETWEEN 1 AND 5");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.YieldCapacity).HasMaxLength(500);
        builder.Property(x => x.MainMarket).HasMaxLength(500);
        builder.Property(x => x.EvaluatedBy).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        
        builder.Property(x => x.ImportedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // UQ
        builder.HasIndex(x => new { x.ProductId, x.EvaluatedYear })
            .IsUnique()
            .HasDatabaseName("UQ_ProductOcop_Year");

        // Relationships
        builder.HasOne(x => x.Product)
            .WithMany(x => x.ProductOcopHistories)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CraftVillage)
            .WithMany(x => x.ProductOcopHistories)
            .HasForeignKey(x => x.CraftVillageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
