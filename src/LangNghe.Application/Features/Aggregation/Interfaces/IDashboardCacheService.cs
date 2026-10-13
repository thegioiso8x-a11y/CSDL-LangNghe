using LangNghe.Application.Features.Aggregation.Queries;

namespace LangNghe.Application.Features.Aggregation.Interfaces;

/// <summary>
/// Dịch vụ quản lý cache cho Dashboard
/// </summary>
public interface IDashboardCacheService
{
    Task<DashboardMetricsDto?> GetAsync(string period, byte periodType, int regionId, CancellationToken ct);
    Task SetAsync(string period, byte periodType, int regionId, DashboardMetricsDto data, CancellationToken ct);
    void Invalidate(string period, byte periodType, int regionId);
}
