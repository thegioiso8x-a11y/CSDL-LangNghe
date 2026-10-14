using System.Reflection;
using LangNghe.Domain.Entities;
using LangNghe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Infrastructure.Persistence;

public class LangNgheDbContext : DbContext, ILangNgheDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public LangNgheDbContext(
        DbContextOptions<LangNgheDbContext> options,
        ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
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

        // Global Query Filters (DataScope rules for Region ownership & Soft Delete)
        var dataScope = _currentUserService.DataScope;
        var userRegionId = _currentUserService.RegionId;

        // CraftVillage Filter
        builder.Entity<CraftVillage>().HasQueryFilter(x =>
            dataScope == null                                          // seed/job không filter
         || dataScope == 1                                             // TW thấy hết
         || (dataScope == 2 && x.Region!.ParentId == userRegionId)     // Tỉnh: các Xã thuộc tỉnh mình
         || (dataScope == 3 && x.RegionId == userRegionId));           // Xã: chỉ làng nghề của xã mình

        // Region Filter (Soft delete)
        builder.Entity<Region>().HasQueryFilter(x => x.IsActive);
        
        // System users filter
        builder.Entity<User>().HasQueryFilter(x => x.IsActive);
    }
}
