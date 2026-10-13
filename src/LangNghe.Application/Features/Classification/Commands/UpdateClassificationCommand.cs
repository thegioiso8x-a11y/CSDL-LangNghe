using LangNghe.Domain.Exceptions;
using LangNghe.Application.Features.Classification.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Classification.Commands;

public record UpdateClassificationCommand(int Id, UpdateClassificationDto Dto) : IRequest<bool>;

public class UpdateClassificationCommandHandler : IRequestHandler<UpdateClassificationCommand, bool>
{
    private readonly ILangNgheDbContext _context;

    public UpdateClassificationCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateClassificationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Classifications
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.Classification), request.Id);

        var exists = await _context.Classifications.AnyAsync(x => x.Code == request.Dto.Code && x.Id != request.Id, cancellationToken);
        if (exists)
            throw new BusinessException("Mã đã tồn tại ở bản ghi khác.");

        entity.Code = request.Dto.Code;
        entity.Name = request.Dto.Name;
        entity.SortOrder = request.Dto.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
