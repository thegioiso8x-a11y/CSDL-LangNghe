using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Dim_Products", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CraftVillageId)
            .IsRequired();

        builder.Property(x => x.ProductCode)
            .IsRequired(false)
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Description)
            .IsRequired(false)
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(x => x.Materials)
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(x => x.ImageUrl)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(x => x.HasOCOP)
            .IsRequired();

        builder.Property(x => x.OcopStar)
            .IsRequired(false);

        builder.Property(x => x.OcopEvaluatedYear)
            .IsRequired(false);

        builder.Property(x => x.MainMarket)
            .IsRequired(false)
            .HasMaxLength(500);

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
        builder.HasOne(x => x.CraftVillage)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CraftVillageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => new { x.CraftVillageId, x.IsActive })
            .HasDatabaseName("IX_Products_Village")
            .IncludeProperties(x => new { x.Name, x.OcopStar });

        // Checks
        builder.ToTable(t => t.HasCheckConstraint("CK_Dim_Products_OcopStar", "OcopStar IS NULL OR OcopStar IN (3,4,5)"));
    }
}