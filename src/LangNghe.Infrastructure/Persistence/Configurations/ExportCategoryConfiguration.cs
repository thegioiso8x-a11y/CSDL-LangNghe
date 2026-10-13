using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class ExportCategoryConfiguration : IEntityTypeConfiguration<ExportCategory>
{
    public void Configure(EntityTypeBuilder<ExportCategory> builder)
    {
        builder.ToTable("Dim_ExportCategories", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.SortOrder)
            .IsRequired(false);

        builder.Property(x => x.IsActive)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("UQ_Dim_ExportCategories_Code");
    }
}