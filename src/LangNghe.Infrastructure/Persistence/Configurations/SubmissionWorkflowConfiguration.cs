using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LangNghe.Infrastructure.Persistence.Configurations;

public class SubmissionWorkflowConfiguration : IEntityTypeConfiguration<SubmissionWorkflow>
{
    public void Configure(EntityTypeBuilder<SubmissionWorkflow> builder)
    {
        builder.ToTable("Fact_SubmissionWorkflow", "dbo", t =>
        {
            t.HasCheckConstraint("CK_SubmissionStatus", "CurrentStatus IN (1,2,3,4)");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReportPeriod).IsRequired().HasMaxLength(10).IsUnicode(false);
        builder.Property(x => x.RejectionReason).HasMaxLength(1000);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // UQ
        builder.HasIndex(x => new { x.CraftVillageId, x.ReportPeriod, x.Version })
            .IsUnique()
            .HasDatabaseName("UQ_SubmissionWorkflow_Version");

        // Relationships
        builder.HasOne(x => x.CraftVillage)
            .WithMany(x => x.SubmissionWorkflows)
            .HasForeignKey(x => x.CraftVillageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UserXa)
            .WithMany(x => x.SubmissionWorkflowsXa)
            .HasForeignKey(x => x.CreatedByXa)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UserTinh)
            .WithMany(x => x.SubmissionWorkflowsTinh)
            .HasForeignKey(x => x.ApprovedByTinh)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UserTW)
            .WithMany(x => x.SubmissionWorkflowsTW)
            .HasForeignKey(x => x.PublishedByTW)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UserReject)
            .WithMany(x => x.SubmissionWorkflowsReject)
            .HasForeignKey(x => x.RejectedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
