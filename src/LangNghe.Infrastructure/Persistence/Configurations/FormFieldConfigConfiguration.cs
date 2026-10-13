namespace LangNghe.Infrastructure.Persistence.Configurations;

using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FormFieldConfigConfiguration : IEntityTypeConfiguration<FormFieldConfig>
{
    public void Configure(EntityTypeBuilder<FormFieldConfig> builder)
    {
        builder.ToTable("Sys_FormFieldConfig", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FieldName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.TabName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ControlType)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(x => x.DropdownSource)
            .HasMaxLength(200);

        builder.Property(x => x.IsRequired)
            .HasDefaultValue(false);

        builder.Property(x => x.DefaultValue)
            .HasMaxLength(500);

        builder.Property(x => x.Placeholder)
            .HasMaxLength(500);

        builder.Property(x => x.HelpText)
            .HasMaxLength(1000);

        builder.Property(x => x.MinValue)
            .HasColumnType("DECIMAL(18,2)");

        builder.Property(x => x.MaxValue)
            .HasColumnType("DECIMAL(18,2)");

        builder.Property(x => x.VisibleForScopes)
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(x => x.EditableForScopes)
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => x.FieldName)
            .IsUnique()
            .HasDatabaseName("UQ_FormField_FieldName");
    }
}
