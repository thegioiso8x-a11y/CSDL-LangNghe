using System.Threading;
using System.Threading.Tasks;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Region.Commands;

public record DeleteRegionCommand(int Id) : IRequest<bool>;

public class DeleteRegionCommandHandler : IRequestHandler<DeleteRegionCommand, bool>
{
    private readonly ILangNgheDbContext _context;

    public DeleteRegionCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteRegionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Regions
            .Include(x => x.Children)
            .Include(x => x.CraftVillages)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.Region), request.Id);

        if (entity.LevelType == 1 && entity.Children.Count > 0)
        {
            var count = entity.Children.Count;
            throw new BusinessException($"Không thể xóa: có {count} xã trực thuộc.");
        }

        if (entity.LevelType == 2 && entity.CraftVillages.Count > 0)
        {
            var count = entity.CraftVillages.Count;
            throw new BusinessException($"Không thể xóa: có {count} làng nghề thuộc xã này.");
        }

        entity.IsActive = false;
        
        _context.Regions.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
