using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Classification.Commands;

public record DeleteClassificationCommand(int Id) : IRequest<bool>;

public class DeleteClassificationCommandHandler : IRequestHandler<DeleteClassificationCommand, bool>
{
    private readonly ILangNgheDbContext _context;

    public DeleteClassificationCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteClassificationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Classifications
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.Classification), request.Id);

        var count = await _context.CraftVillages.CountAsync(v => v.ClassificationId == request.Id, cancellationToken);
        if (count > 0)
            throw new BusinessException($"Không thể xóa: có {count} làng nghề đang sử dụng");

        _context.Classifications.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
