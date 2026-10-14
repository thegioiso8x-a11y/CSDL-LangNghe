namespace LangNghe.Jobs.Jobs;

using LangNghe.Application.Features.Aggregation.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Job rebuild các bảng tổng hợp (Agg_*) từ Fact table. Chạy 2h sáng hàng ngày.
/// </summary>
public class RebuildAggJob
{
    private readonly IAggRebuildService _aggService;
    private readonly ILogger<RebuildAggJob> _logger;

    public RebuildAggJob(IAggRebuildService aggService, ILogger<RebuildAggJob> logger)
    {
        _aggService = aggService;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        var currentYear = DateTime.UtcNow.Year.ToString();
        _logger.LogInformation("Bắt đầu rebuild Agg cho kỳ {Period} lúc {Time}", currentYear, DateTime.UtcNow);

        try
        {
            await _aggService.RebuildAllAsync(currentYear, 3, CancellationToken.None);
            _logger.LogInformation("Hoàn tất rebuild Agg cho kỳ {Period}", currentYear);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi rebuild Agg cho kỳ {Period}", currentYear);
            throw;
        }
    }
}
