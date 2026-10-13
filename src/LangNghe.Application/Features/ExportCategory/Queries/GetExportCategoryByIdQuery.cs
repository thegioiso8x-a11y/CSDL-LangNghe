using LangNghe.Application.Features.ExportCategory.Dtos;
using LangNghe.Domain.Interfaces;
using LangNghe.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.ExportCategory.Queries;

/// <summary>
/// Truy vấn Nhóm mặt hàng xuất khẩu theo ID.
/// </summary>
public record GetExportCategoryByIdQuery(int Id) : IRequest<ExportCategoryDto>;

public class GetExportCategoryByIdQueryHandler : IRequestHandler<GetExportCategoryByIdQuery, ExportCategoryDto>
{
    private readonly ILangNgheDbContext _context;

    public GetExportCategoryByIdQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<ExportCategoryDto> Handle(GetExportCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.ExportCategories
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new ExportCategoryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(Domain.Entities.ExportCategory), request.Id);

        return entity;
    }
}
