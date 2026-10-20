namespace LangNghe.Jobs;

using Hangfire;
using Hangfire.SqlServer;
using LangNghe.Jobs.Jobs;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension đăng ký Hangfire server, storage và các job định kỳ.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký Hangfire server + storage + 3 job scoped.
    /// </summary>
    public static IServiceCollection AddHangfireJobs(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

        services.AddHangfireServer();

        services.AddScoped<RebuildAggJob>();
        services.AddScoped<CleanupTokenJob>();
        services.AddScoped<CleanupAuditJob>();
        services.AddScoped<BatchGeocodeJob>();

        return services;
    }

    /// <summary>
    /// Schedule 3 recurring job. Gọi 1 lần khi app start.
    /// </summary>
    public static void ScheduleHangfireJobs(this IServiceProvider services)
    {
        var manager = services.GetRequiredService<IRecurringJobManager>();

        manager.AddOrUpdate<RebuildAggJob>(
            "rebuild-agg-daily",
            job => job.RunAsync(),
            "0 2 * * *");

        manager.AddOrUpdate<CleanupTokenJob>(
            "cleanup-token-daily",
            job => job.RunAsync(),
            "0 3 * * *");

        manager.AddOrUpdate<CleanupAuditJob>(
            "cleanup-audit-daily",
            job => job.RunAsync(),
            "0 4 * * *");

        manager.AddOrUpdate<BatchGeocodeJob>(
            "batch-geocode-villages",
            job => job.RunAsync(CancellationToken.None),
            "0 1 * * *");
    }
}
