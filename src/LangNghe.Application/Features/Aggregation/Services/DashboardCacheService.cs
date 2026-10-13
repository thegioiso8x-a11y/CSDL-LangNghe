using LangNghe.Application.Features.Aggregation.Interfaces;
using LangNghe.Application.Features.Aggregation.Queries;
using Microsoft.Extensions.Caching.Memory;

namespace LangNghe.Application.Features.Aggregation.Services;

/// <summary>
/// Triển khai dịch vụ cache cho Dashboard
/// </summary>
public class DashboardCacheService : IDashboardCacheService
{
    private readonly IMemoryCache _cache;

    public DashboardCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    private string GetCacheKey(string period, byte periodType, int regionId)
        => $"dashboard:{period}:{periodType}:{regionId}";

    public Task<DashboardMetricsDto?> GetAsync(string period, byte periodType, int regionId, CancellationToken ct)
    {
        var key = GetCacheKey(period, periodType, regionId);
        _cache.TryGetValue(key, out DashboardMetricsDto? data);
        return Task.FromResult(data);
    }

    public Task SetAsync(string period, byte periodType, int regionId, DashboardMetricsDto data, CancellationToken ct)
    {
        var key = GetCacheKey(period, periodType, regionId);
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
        };
        _cache.Set(key, data, options);
        return Task.CompletedTask;
    }

    public void Invalidate(string period, byte periodType, int regionId)
    {
        var key = GetCacheKey(period, periodType, regionId);
        _cache.Remove(key);
    }
}
