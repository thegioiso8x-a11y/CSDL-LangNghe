namespace LangNghe.Application.Features.Aggregation.Interfaces;

using System.Threading;
using System.Threading.Tasks;

public interface IAggRebuildService
{
    Task RebuildTimeSeriesAsync(string period, byte periodType, CancellationToken ct);
    Task RebuildIndustryGroupsAsync(string period, byte periodType, CancellationToken ct);
    Task RebuildVillageTypesAsync(string period, byte periodType, CancellationToken ct);
    Task RebuildAllAsync(string period, byte periodType, CancellationToken ct);
}
