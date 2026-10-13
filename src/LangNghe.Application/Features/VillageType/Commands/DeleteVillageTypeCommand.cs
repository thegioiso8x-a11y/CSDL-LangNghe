using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.VillageType.Commands;

public record DeleteVillageTypeCommand(int Id) : IRequest<bool>;

public class DeleteVillageTypeCommandHandler : IRequestHandler<DeleteVillageTypeCommand, bool>
{
    private readonly ILangNgheDbContext _context;

    public DeleteVillageTypeCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteVillageTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.VillageTypes
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.VillageType), request.Id);

        entity.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
