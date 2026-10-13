using LangNghe.Application.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using LangNghe.Application.Features.Draft.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Draft.Queries;

public record GetDraftsByUserQuery(DraftFilterDto Filter) : IRequest<ApiResponse<PagedResult<DraftDto>>>;

public class GetDraftsByUserQueryHandler : IRequestHandler<GetDraftsByUserQuery, ApiResponse<PagedResult<DraftDto>>>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public GetDraftsByUserQueryHandler(ILangNgheDbContext context, IMapper mapper, ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResult<DraftDto>>> Handle(GetDraftsByUserQuery request, CancellationToken cancellationToken)
    {
        var query = _context.VillageDrafts
            .AsNoTracking()
            .Where(x => x.CreatedByUserId == _currentUser.UserId);

        if (request.Filter.CraftVillageId.HasValue)
            query = query.Where(x => x.CraftVillageId == request.Filter.CraftVillageId);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize)
            .ProjectTo<DraftDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return ApiResponse<PagedResult<DraftDto>>.Ok(new PagedResult<DraftDto>(items, request.Filter.Page, request.Filter.PageSize, totalCount));
    }
}
