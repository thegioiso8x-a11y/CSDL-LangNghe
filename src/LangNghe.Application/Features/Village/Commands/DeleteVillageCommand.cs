using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Village.Commands;

public record DeleteVillageCommand(int Id) : IRequest<bool>;

public class DeleteVillageCommandHandler : IRequestHandler<DeleteVillageCommand, bool>
{
    private readonly ILangNgheDbContext _context;

    public DeleteVillageCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteVillageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CraftVillages.AsTracking()
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException("CraftVillage", request.Id);
        }

        var productsCount = await _context.Products.CountAsync(p => p.CraftVillageId == request.Id, cancellationToken);
        var artisansCount = await _context.Artisans.CountAsync(a => a.CraftVillageId == request.Id, cancellationToken);
        var metricsCount = await _context.VillagePeriodicMetrics.CountAsync(m => m.CraftVillageId == request.Id, cancellationToken);

        var totalRelations = productsCount + artisansCount + metricsCount;
        if (totalRelations > 0)
        {
            throw new BusinessException($"Không thể xóa: có {totalRelations} dữ liệu liên quan");
        }

        _context.CraftVillages.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
