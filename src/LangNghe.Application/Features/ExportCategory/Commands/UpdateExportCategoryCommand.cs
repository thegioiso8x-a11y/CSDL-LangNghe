using LangNghe.Domain.Exceptions;
using LangNghe.Application.Features.ExportCategory.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.ExportCategory.Commands;

/// <summary>
/// Lệnh cập nhật Nhóm mặt hàng xuất khẩu.
/// </summary>
public record UpdateExportCategoryCommand(int Id, UpdateExportCategoryDto Dto) : IRequest<bool>;

public class UpdateExportCategoryCommandHandler : IRequestHandler<UpdateExportCategoryCommand, bool>
{
    private readonly ILangNgheDbContext _context;

    public UpdateExportCategoryCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateExportCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ExportCategories
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.ExportCategory), request.Id);

        var exists = await _context.ExportCategories.AnyAsync(x => x.Code == request.Dto.Code && x.Id != request.Id, cancellationToken);
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
