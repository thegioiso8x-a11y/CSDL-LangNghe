using AutoMapper;
using AutoMapper.QueryableExtensions;
using LangNghe.Application.Common;
using LangNghe.Application.Features.Classification.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Classification.Queries;

public record GetClassificationListQuery(ClassificationFilterDto Filter) : IRequest<PagedResult<ClassificationDto>>;

public class GetClassificationListQueryHandler : IRequestHandler<GetClassificationListQuery, PagedResult<ClassificationDto>>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;

    public GetClassificationListQueryHandler(ILangNgheDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<ClassificationDto>> Handle(GetClassificationListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Classifications.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Filter.Keyword))
        {
            var keyword = request.Filter.Keyword.ToLower();
            query = query.Where(x => x.Code.ToLower().Contains(keyword) || x.Name.ToLower().Contains(keyword));
        }

        

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Code)
            .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize)
            .ProjectTo<ClassificationDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<ClassificationDto>(items, request.Filter.Page, request.Filter.PageSize, totalCount);
    }
}
