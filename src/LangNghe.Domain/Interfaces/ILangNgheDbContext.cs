using LangNghe.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Domain.Interfaces;

/// <summary>
/// Interface expose DbSet + SaveChanges cho Application layer sử dụng
/// mà không cần reference Infrastructure.
/// </summary>
public interface ILangNgheDbContext
{
    DbSet<Region> Regions { get; }
    DbSet<RegionMapping> RegionMappings { get; }
    DbSet<VillageType> VillageTypes { get; }
    DbSet<IndustryGroup> IndustryGroups { get; }
    DbSet<OutputUnit> OutputUnits { get; }
    DbSet<ExportCategory> ExportCategories { get; }
    DbSet<Classification> Classifications { get; }
    DbSet<CraftVillage> CraftVillages { get; }
    DbSet<Product> Products { get; }
    DbSet<Artisan> Artisans { get; }
    DbSet<VillagePeriodicMetric> VillagePeriodicMetrics { get; }
    DbSet<ProductOcopHistory> ProductOcopHistories { get; }
    DbSet<VillageEnvironment> VillageEnvironments { get; }
    DbSet<SubmissionWorkflow> SubmissionWorkflows { get; }
    DbSet<VillageDraft> VillageDrafts { get; }
    DbSet<VillageTimeSeries> VillageTimeSeries { get; }
    DbSet<IndustryGroupAggregate> IndustryGroupAggregates { get; }
    DbSet<VillageTypeAggregate> VillageTypeAggregates { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<FormFieldConfig> FormFieldConfigs { get; }
    DbSet<ImportHistory> ImportHistories { get; }
    DbSet<RawImportStaging> RawImportStagings { get; }
    DbSet<ImportErrorLog> ImportErrorLogs { get; }
    DbSet<ImportTemplateConfig> ImportTemplateConfigs { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
