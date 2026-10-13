using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class OutputUnitConfiguration : IEntityTypeConfiguration<OutputUnit>
{
    public void Configure(EntityTypeBuilder<OutputUnit> builder)
    {
        builder.ToTable("Dim_OutputUnits", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SortOrder)
            .IsRequired(false);

        builder.Property(x => x.IsActive)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("UQ_Dim_OutputUnits_Code");
    }
}