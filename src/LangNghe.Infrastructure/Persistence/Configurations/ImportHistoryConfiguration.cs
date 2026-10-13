namespace LangNghe.Infrastructure.Persistence.Configurations;

using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ImportHistoryConfiguration : IEntityTypeConfiguration<ImportHistory>
{
    public void Configure(EntityTypeBuilder<ImportHistory> builder)
    {
        builder.ToTable("Import_History", "dbo", t =>
        {
            t.HasCheckConstraint("CK_Import_Status", "Status IN (1,2,3,4,5)");
        });

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.SheetName)
            .HasMaxLength(100);

        builder.Property(x => x.FileHash)
            .HasMaxLength(64)
            .IsUnicode(false);

        builder.Property(x => x.BlobPath)
            .HasMaxLength(500);

        builder.Property(x => x.TemplateCode)
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(x => x.ImportMode)
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(x => x.TotalRows)
            .HasDefaultValue(0);

        builder.Property(x => x.SuccessRows)
            .HasDefaultValue(0);

        builder.Property(x => x.ErrorRows)
            .HasDefaultValue(0);

        builder.Property(x => x.WarningRows)
            .HasDefaultValue(0);

        builder.Property(x => x.Status)
            .HasDefaultValue((byte)1);

        builder.Property(x => x.StartedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.UploadedByUser)
            .WithMany(x => x.ImportHistories)
            .HasForeignKey(x => x.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Import_User");

        builder.HasOne(x => x.UploadedByRegion)
            .WithMany(x => x.ImportHistories)
            .HasForeignKey(x => x.UploadedByRegionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Import_Region");
    }
}
