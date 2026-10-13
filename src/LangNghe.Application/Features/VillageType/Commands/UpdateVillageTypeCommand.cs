using LangNghe.Domain.Exceptions;
using LangNghe.Application.Features.VillageType.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.VillageType.Commands;

public record UpdateVillageTypeCommand(int Id, UpdateVillageTypeDto Dto) : IRequest<bool>;

public class UpdateVillageTypeCommandHandler : IRequestHandler<UpdateVillageTypeCommand, bool>
{
    private readonly ILangNgheDbContext _context;

    public UpdateVillageTypeCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateVillageTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.VillageTypes
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.VillageType), request.Id);

        var exists = await _context.VillageTypes.AnyAsync(x => x.Code == request.Dto.Code && x.Id != request.Id, cancellationToken);
        if (exists)
            throw new BusinessException("Mã đã tồn tại ở bản ghi khác.");

        entity.Code = request.Dto.Code;
        entity.Name = request.Dto.Name;
        entity.IsTraditional = request.Dto.IsTraditional;
        entity.IsRestored = request.Dto.IsRestored;
        entity.IsNew = request.Dto.IsNew;
        entity.SortOrder = request.Dto.SortOrder;
        entity.IsActive = request.Dto.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
