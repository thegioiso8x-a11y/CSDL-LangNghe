using LangNghe.Application.Common;
using LangNghe.Application.Features.ExportCategory.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.ExportCategory.Queries;

/// <summary>
/// Truy vấn danh sách Nhóm mặt hàng xuất khẩu.
/// </summary>
public record GetExportCategoryListQuery(ExportCategoryFilterDto Filter) : IRequest<PagedResult<ExportCategoryDto>>;

public class GetExportCategoryListQueryHandler : IRequestHandler<GetExportCategoryListQuery, PagedResult<ExportCategoryDto>>
{
    private readonly ILangNgheDbContext _context;

    public GetExportCategoryListQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ExportCategoryDto>> Handle(GetExportCategoryListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ExportCategories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Filter.Keyword))
        {
            var keyword = request.Filter.Keyword.ToLower();
            query = query.Where(x => x.Code.ToLower().Contains(keyword) || x.Name.ToLower().Contains(keyword));
        }

        if (request.Filter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.Filter.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Code)
            .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize)
            .Select(x => new ExportCategoryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ExportCategoryDto>(items, request.Filter.Page, request.Filter.PageSize, totalCount);
    }
}
