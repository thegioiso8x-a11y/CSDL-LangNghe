namespace LangNghe.Infrastructure.Persistence.Configurations;

using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RawImportStagingConfiguration : IEntityTypeConfiguration<RawImportStaging>
{
    public void Configure(EntityTypeBuilder<RawImportStaging> builder)
    {
        builder.ToTable("Staging_RawImport", "dbo", t =>
        {
            t.HasCheckConstraint("CK_Staging_Status", "ValidationStatus IN (0,1,2,3)");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SheetName)
            .HasMaxLength(100);

        builder.Property(x => x.RawData)
            .IsRequired();

        builder.Property(x => x.ParsedVillageCode)
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(x => x.ValidationStatus)
            .HasDefaultValue((byte)0);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.ImportHistory)
            .WithMany(x => x.RawImportStagings)
            .HasForeignKey(x => x.ImportBatchId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Staging_Import");
    }
}
