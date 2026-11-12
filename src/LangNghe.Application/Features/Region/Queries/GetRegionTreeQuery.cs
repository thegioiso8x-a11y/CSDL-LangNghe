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
    private readonly ICurrentUserService _currentUser;

    public GetRegionTreeQueryHandler(ILangNgheDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<RegionTreeDto>> Handle(GetRegionTreeQuery request, CancellationToken cancellationToken)
    {
        var dataScope = _currentUser.DataScope;
        var userRegionId = _currentUser.RegionId;

        // ===== Data Scope Isolation: Tỉnh/Xã chỉ thấy Tỉnh mẹ + các Xã con thuộc quyền quản lý =====
        int? allowedProvinceId = null;
        if (dataScope == 2 && userRegionId.HasValue)
        {
            // Tỉnh: chỉ tỉnh mình
            allowedProvinceId = userRegionId.Value;
        }
        else if (dataScope == 3 && userRegionId.HasValue)
        {
            // Xã: lấy tỉnh mẹ của xã mình
            allowedProvinceId = await _context.Regions.AsNoTracking()
                .Where(x => x.Id == userRegionId.Value)
                .Select(x => x.ParentId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var regionsQuery = _context.Regions
            .AsNoTracking()
            .Where(x => x.IsActive);

        if (allowedProvinceId.HasValue)
        {
            // Chỉ lấy tỉnh được phép và các xã con của tỉnh đó
            regionsQuery = regionsQuery.Where(x => x.Id == allowedProvinceId.Value || x.ParentId == allowedProvinceId.Value);
        }

        var allRegions = await regionsQuery
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
