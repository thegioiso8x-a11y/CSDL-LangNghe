using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Features.Region.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Region.Queries;

public record GetRegionTreeQuery() : IRequest<List<RegionTreeDto>>;

public class GetRegionTreeQueryHandler : IRequestHandler<GetRegionTreeQuery, List<RegionTreeDto>>
{
    private readonly ILangNgheDbContext _context;

    public GetRegionTreeQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<List<RegionTreeDto>> Handle(GetRegionTreeQuery request, CancellationToken cancellationToken)
    {
        var allRegions = await _context.Regions
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.RegionCode)
            .Select(x => new
            {
                x.Id,
                x.RegionCode,
                x.Name,
                x.LevelType,
                x.ParentId
            })
            .ToListAsync(cancellationToken);

        var tree = allRegions
            .Where(x => x.ParentId == null && x.LevelType == 1)
            .Select(x => new RegionTreeDto
            {
                Id = x.Id,
                RegionCode = x.RegionCode,
                Name = x.Name,
                LevelType = x.LevelType,
                Children = allRegions
                    .Where(c => c.ParentId == x.Id && c.LevelType == 2)
                    .Select(c => new RegionTreeDto
                    {
                        Id = c.Id,
                        RegionCode = c.RegionCode,
                        Name = c.Name,
                        LevelType = c.LevelType,
                        Children = new List<RegionTreeDto>()
                    })
                    .ToList()
            })
            .ToList();

        return tree;
    }
}
