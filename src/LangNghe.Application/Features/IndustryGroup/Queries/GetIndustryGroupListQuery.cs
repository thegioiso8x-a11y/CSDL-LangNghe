using AutoMapper;
using AutoMapper.QueryableExtensions;
using LangNghe.Application.Common;
using LangNghe.Application.Features.IndustryGroup.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.IndustryGroup.Queries;

public record GetIndustryGroupListQuery(IndustryGroupFilterDto Filter) : IRequest<PagedResult<IndustryGroupDto>>;

public class GetIndustryGroupListQueryHandler : IRequestHandler<GetIndustryGroupListQuery, PagedResult<IndustryGroupDto>>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;

    public GetIndustryGroupListQueryHandler(ILangNgheDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<IndustryGroupDto>> Handle(GetIndustryGroupListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.IndustryGroups.AsNoTracking();

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
            .ProjectTo<IndustryGroupDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<IndustryGroupDto>(items, request.Filter.Page, request.Filter.PageSize, totalCount);
    }
}
