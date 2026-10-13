using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.OutputUnit.Commands;

/// <summary>
/// Lệnh xóa (xóa mềm) Đơn vị tính sản lượng.
/// </summary>
public record DeleteOutputUnitCommand(int Id) : IRequest<bool>;

public class DeleteOutputUnitCommandHandler : IRequestHandler<DeleteOutputUnitCommand, bool>
{
    private readonly ILangNgheDbContext _context;

    public DeleteOutputUnitCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteOutputUnitCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.OutputUnits
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.OutputUnit), request.Id);

        entity.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
