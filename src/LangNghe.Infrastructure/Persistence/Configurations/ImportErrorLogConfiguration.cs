namespace LangNghe.Infrastructure.Persistence.Configurations;

using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ImportErrorLogConfiguration : IEntityTypeConfiguration<ImportErrorLog>
{
    public void Configure(EntityTypeBuilder<ImportErrorLog> builder)
    {
        builder.ToTable("Import_Error_Log", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ColumnName)
            .HasMaxLength(100);

        builder.Property(x => x.ErrorCode)
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(x => x.ErrorMessage)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.RawValue)
            .HasMaxLength(500);

        builder.Property(x => x.Severity)
            .HasDefaultValue((byte)1);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.ImportHistory)
            .WithMany(x => x.ImportErrorLogs)
            .HasForeignKey(x => x.ImportBatchId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_ImportError_Import");
    }
}
