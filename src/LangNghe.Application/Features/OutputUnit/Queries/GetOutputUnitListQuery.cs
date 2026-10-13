using LangNghe.Application.Common;
using LangNghe.Application.Features.OutputUnit.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.OutputUnit.Queries;

/// <summary>
/// Truy vấn danh sách Đơn vị tính sản lượng.
/// </summary>
public record GetOutputUnitListQuery(OutputUnitFilterDto Filter) : IRequest<PagedResult<OutputUnitDto>>;

public class GetOutputUnitListQueryHandler : IRequestHandler<GetOutputUnitListQuery, PagedResult<OutputUnitDto>>
{
    private readonly ILangNgheDbContext _context;

    public GetOutputUnitListQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<OutputUnitDto>> Handle(GetOutputUnitListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.OutputUnits.AsNoTracking();

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
            .Select(x => new OutputUnitDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<OutputUnitDto>(items, request.Filter.Page, request.Filter.PageSize, totalCount);
    }
}
