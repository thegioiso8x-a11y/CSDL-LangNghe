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

    public RebuildAggCommandHandler(IAggRebuildService aggRebuildService)
    {
        _aggRebuildService = aggRebuildService;
    }

    public async Task<ApiResponse<bool>> Handle(RebuildAggCommand request, CancellationToken cancellationToken)
    {
        await _aggRebuildService.RebuildAllAsync(request.Period, request.PeriodType, cancellationToken);
        return ApiResponse<bool>.Ok(true, "Rebuild aggregation data successfully");
    }
}
