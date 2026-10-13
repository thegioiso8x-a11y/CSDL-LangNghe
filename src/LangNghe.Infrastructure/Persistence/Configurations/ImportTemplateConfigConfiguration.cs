namespace LangNghe.Infrastructure.Persistence.Configurations;

using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ImportTemplateConfigConfiguration : IEntityTypeConfiguration<ImportTemplateConfig>
{
    public void Configure(EntityTypeBuilder<ImportTemplateConfig> builder)
    {
        builder.ToTable("Import_TemplateConfig", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TemplateCode)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(x => x.FieldName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ExcelColumnHeader)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.IsRequired)
            .HasDefaultValue(false);

        builder.Property(x => x.DefaultValue)
            .HasMaxLength(500);

        builder.Property(x => x.DataType)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(x => x.ValidationRegex)
            .HasMaxLength(500)
            .IsUnicode(false);

        builder.Property(x => x.GroupName)
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.HelpText)
            .HasMaxLength(1000);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(x => new { x.TemplateCode, x.FieldName })
            .IsUnique()
            .HasDatabaseName("UQ_Template_Field");
    }
}
