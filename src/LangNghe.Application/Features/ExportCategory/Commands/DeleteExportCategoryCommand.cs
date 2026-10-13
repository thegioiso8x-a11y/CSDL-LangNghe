using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.ExportCategory.Commands;

/// <summary>
/// Lệnh xóa (xóa mềm) Nhóm mặt hàng xuất khẩu.
/// </summary>
public record DeleteExportCategoryCommand(int Id) : IRequest<bool>;

public class DeleteExportCategoryCommandHandler : IRequestHandler<DeleteExportCategoryCommand, bool>
{
    private readonly ILangNgheDbContext _context;

    public DeleteExportCategoryCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteExportCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ExportCategories
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.ExportCategory), request.Id);

        entity.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
