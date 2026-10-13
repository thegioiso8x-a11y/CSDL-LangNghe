using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class IndustryGroupConfiguration : IEntityTypeConfiguration<IndustryGroup>
{
    public void Configure(EntityTypeBuilder<IndustryGroup> builder)
    {
        builder.ToTable("Dim_IndustryGroups", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Description)
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(x => x.SortOrder)
            .IsRequired(false);

        builder.Property(x => x.IsActive)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("UQ_Dim_IndustryGroups_Code");
    }
}