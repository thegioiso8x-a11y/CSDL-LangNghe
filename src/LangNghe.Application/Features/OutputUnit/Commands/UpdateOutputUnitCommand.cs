using LangNghe.Domain.Exceptions;
using LangNghe.Application.Features.OutputUnit.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.OutputUnit.Commands;

/// <summary>
/// Lệnh cập nhật Đơn vị tính sản lượng.
/// </summary>
public record UpdateOutputUnitCommand(int Id, UpdateOutputUnitDto Dto) : IRequest<bool>;

public class UpdateOutputUnitCommandHandler : IRequestHandler<UpdateOutputUnitCommand, bool>
{
    private readonly ILangNgheDbContext _context;

    public UpdateOutputUnitCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateOutputUnitCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.OutputUnits
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.OutputUnit), request.Id);

        var exists = await _context.OutputUnits.AnyAsync(x => x.Code == request.Dto.Code && x.Id != request.Id, cancellationToken);
        if (exists)
            throw new BusinessException("Mã đã tồn tại ở bản ghi khác.");

        entity.Code = request.Dto.Code;
        entity.Name = request.Dto.Name;
        entity.SortOrder = request.Dto.SortOrder;
        entity.IsActive = request.Dto.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
