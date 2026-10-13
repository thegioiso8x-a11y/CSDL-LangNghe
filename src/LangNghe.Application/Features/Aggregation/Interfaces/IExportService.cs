using System.Threading;
using System.Threading.Tasks;

namespace LangNghe.Application.Features.Aggregation.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportSummaryAsync(string period, byte periodType, int regionId, CancellationToken ct);
    Task<byte[]> ExportDetailAsync(string period, byte periodType, int regionId, CancellationToken ct);
}
