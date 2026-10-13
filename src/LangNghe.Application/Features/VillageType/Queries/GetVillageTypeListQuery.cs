using AutoMapper;
using AutoMapper.QueryableExtensions;
using LangNghe.Application.Common;
using LangNghe.Application.Features.VillageType.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.VillageType.Queries;

public record GetVillageTypeListQuery(VillageTypeFilterDto Filter) : IRequest<PagedResult<VillageTypeDto>>;

public class GetVillageTypeListQueryHandler : IRequestHandler<GetVillageTypeListQuery, PagedResult<VillageTypeDto>>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;

    public GetVillageTypeListQueryHandler(ILangNgheDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<VillageTypeDto>> Handle(GetVillageTypeListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.VillageTypes.AsNoTracking();

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
            .ProjectTo<VillageTypeDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<VillageTypeDto>(items, request.Filter.Page, request.Filter.PageSize, totalCount);
    }
}
