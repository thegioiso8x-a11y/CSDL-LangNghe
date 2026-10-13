using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Application.Features.Region.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Region.Queries;

public record GetRegionListQuery(RegionFilterDto Filter) : IRequest<PagedResult<RegionDto>>;

public class GetRegionListQueryHandler : IRequestHandler<GetRegionListQuery, PagedResult<RegionDto>>
{
    private readonly ILangNgheDbContext _context;

    public GetRegionListQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<RegionDto>> Handle(GetRegionListQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.Regions.AsNoTracking();

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        }

        if (filter.LevelType.HasValue)
        {
            query = query.Where(x => x.LevelType == filter.LevelType.Value);
        }

        if (filter.ParentId.HasValue)
        {
            query = query.Where(x => x.ParentId == filter.ParentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(keyword) || x.RegionCode.ToLower().Contains(keyword));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.RegionCode)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new RegionDto
            {
                Id = x.Id,
                RegionCode = x.RegionCode,
                Name = x.Name,
                ShortName = x.ShortName,
                LevelType = x.LevelType,
                RegionType = x.RegionType,
                ParentId = x.ParentId,
                ParentName = x.Parent != null ? x.Parent.Name : null,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<RegionDto>(items, filter.Page, filter.PageSize, totalCount);
    }
}
