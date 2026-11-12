using System.Reflection;
using LangNghe.Domain.Entities;
using LangNghe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Infrastructure.Persistence;

public class LangNgheDbContext : DbContext, ILangNgheDbContext
{
    public LangNgheDbContext(DbContextOptions<LangNgheDbContext> options) : base(options)
    {
    }

    // Dimension
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<RegionMapping> RegionMappings => Set<RegionMapping>();
    public DbSet<VillageType> VillageTypes => Set<VillageType>();
    public DbSet<IndustryGroup> IndustryGroups => Set<IndustryGroup>();
    public DbSet<OutputUnit> OutputUnits => Set<OutputUnit>();
    public DbSet<ExportCategory> ExportCategories => Set<ExportCategory>();
    public DbSet<Classification> Classifications => Set<Classification>();

    // Master
    public DbSet<CraftVillage> CraftVillages => Set<CraftVillage>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Artisan> Artisans => Set<Artisan>();

    // Fact
    public DbSet<VillagePeriodicMetric> VillagePeriodicMetrics => Set<VillagePeriodicMetric>();
    public DbSet<ProductOcopHistory> ProductOcopHistories => Set<ProductOcopHistory>();
    public DbSet<VillageEnvironment> VillageEnvironments => Set<VillageEnvironment>();
    public DbSet<SubmissionWorkflow> SubmissionWorkflows => Set<SubmissionWorkflow>();
    public DbSet<VillageDraft> VillageDrafts => Set<VillageDraft>();

    // Aggregates
    public DbSet<VillageTimeSeries> VillageTimeSeries => Set<VillageTimeSeries>();
    public DbSet<IndustryGroupAggregate> IndustryGroupAggregates => Set<IndustryGroupAggregate>();
    public DbSet<VillageTypeAggregate> VillageTypeAggregates => Set<VillageTypeAggregate>();

    // System
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<FormFieldConfig> FormFieldConfigs => Set<FormFieldConfig>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // Import
    public DbSet<ImportHistory> ImportHistories => Set<ImportHistory>();
    public DbSet<RawImportStaging> RawImportStagings => Set<RawImportStaging>();
    public DbSet<ImportErrorLog> ImportErrorLogs => Set<ImportErrorLog>();
    public DbSet<ImportTemplateConfig> ImportTemplateConfigs => Set<ImportTemplateConfig>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Lưu ý: KHÔNG dùng Global Query Filter cho DataScope nữa (bị cache trong OnModelCreating).
        // DataScope được áp dụng tường minh qua QueryableExtensions.ApplyDataScopeFilter(...) trong các Query Handler.

        // Region Filter (Soft delete)
        builder.Entity<Region>().HasQueryFilter(x => x.IsActive);
        
        // System users filter
        builder.Entity<User>().HasQueryFilter(x => x.IsActive);
    }
}
