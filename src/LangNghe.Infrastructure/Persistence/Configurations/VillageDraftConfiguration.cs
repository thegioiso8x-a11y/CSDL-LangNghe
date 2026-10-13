using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class VillageDraftConfiguration : IEntityTypeConfiguration<VillageDraft>
{
    public void Configure(EntityTypeBuilder<VillageDraft> builder)
    {
        builder.ToTable("Fact_Village_Draft", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DraftVillageCode).HasMaxLength(50);
        builder.Property(x => x.DraftData).IsRequired(); // NVARCHAR(MAX) natively handled
        
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // Relationships
        builder.HasOne(x => x.CraftVillage)
            .WithMany(x => x.VillageDrafts)
            .HasForeignKey(x => x.CraftVillageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.VillageDrafts)
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
