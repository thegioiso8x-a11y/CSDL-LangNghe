using System.Threading;
using System.Threading.Tasks;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.PeriodicMetric.Commands;

public record DeleteMetricCommand(long Id) : IRequest<bool>;

public class DeleteMetricCommandHandler : IRequestHandler<DeleteMetricCommand, bool>
{
    private readonly ILangNgheDbContext _context;
    private readonly IVillageOwnershipService _ownershipService;

    public DeleteMetricCommandHandler(ILangNgheDbContext context, IVillageOwnershipService ownershipService)
    {
        _context = context;
        _ownershipService = ownershipService;
    }

    public async Task<bool> Handle(DeleteMetricCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.VillagePeriodicMetrics
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException("Không tìm thấy chỉ tiêu kỳ báo cáo", request.Id);

        await _ownershipService.ValidateMetricAccessAsync(entity.CraftVillageId, cancellationToken);

        _context.VillagePeriodicMetrics.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
