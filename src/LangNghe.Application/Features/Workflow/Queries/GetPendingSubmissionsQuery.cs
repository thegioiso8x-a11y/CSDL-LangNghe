using LangNghe.Application.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using LangNghe.Application.Features.Workflow.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Workflow.Queries;

public record GetPendingSubmissionsQuery(PageRequest Filter) : IRequest<ApiResponse<PagedResult<SubmissionDto>>>;

public class GetPendingSubmissionsQueryHandler : IRequestHandler<GetPendingSubmissionsQuery, ApiResponse<PagedResult<SubmissionDto>>>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public GetPendingSubmissionsQueryHandler(ILangNgheDbContext context, IMapper mapper, ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResult<SubmissionDto>>> Handle(GetPendingSubmissionsQuery request, CancellationToken cancellationToken)
    {
        var dataScope = _currentUser.DataScope;
        var regionId = _currentUser.RegionId;
        
        var query = _context.SubmissionWorkflows
            .AsNoTracking()
            .Include(x => x.CraftVillage!)
            .ThenInclude(x => x.Region!)
            .AsQueryable();
            
        // Pending logic based on data scope
        if (dataScope == 1) // TW
        {
            query = query.Where(x => x.CurrentStatus == 2); // Pending TW (Approved by Province)
        }
        else if (dataScope == 2) // Province
        {
            query = query.Where(x => x.CurrentStatus == 1 && x.CraftVillage!.Region!.ParentId == regionId); // Pending Province
        }
        else
        {
            // Xã has no pending approval list, return empty
            query = query.Where(x => false);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize)
            .ProjectTo<SubmissionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return ApiResponse<PagedResult<SubmissionDto>>.Ok(new PagedResult<SubmissionDto>(items, request.Filter.Page, request.Filter.PageSize, totalCount));
    }
}
