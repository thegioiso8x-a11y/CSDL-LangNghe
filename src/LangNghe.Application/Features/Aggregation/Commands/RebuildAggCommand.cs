namespace LangNghe.Application.Features.Aggregation.Commands;

using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Application.Features.Aggregation.Interfaces;
using MediatR;

public record RebuildAggCommand(string Period, byte PeriodType) : IRequest<ApiResponse<bool>>;

public class RebuildAggCommandHandler : IRequestHandler<RebuildAggCommand, ApiResponse<bool>>
{
    private readonly IAggRebuildService _aggRebuildService;
    private readonly IDashboardCacheService _cacheService;

    public RebuildAggCommandHandler(IAggRebuildService aggRebuildService, IDashboardCacheService cacheService)
    {
        _aggRebuildService = aggRebuildService;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<bool>> Handle(RebuildAggCommand request, CancellationToken cancellationToken)
    {
        // Rebuild cho TẤT CẢ các kỳ hợp lệ (bao gồm roll-up theo năm) thay vì chỉ kỳ hiện tại.
        await _aggRebuildService.RebuildAllPeriodsAsync(cancellationToken);

        // Xóa toàn bộ cache Dashboard để lần truy vấn kế tiếp lấy dữ liệu mới.
        _cacheService.InvalidateAll();

        return ApiResponse<bool>.Ok(true, "Rebuild aggregation data successfully");
    }
}
