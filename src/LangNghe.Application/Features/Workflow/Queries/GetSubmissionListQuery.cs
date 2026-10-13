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

public record GetSubmissionListQuery(SubmissionFilterDto Filter) : IRequest<ApiResponse<PagedResult<SubmissionDto>>>;

public class GetSubmissionListQueryHandler : IRequestHandler<GetSubmissionListQuery, ApiResponse<PagedResult<SubmissionDto>>>
{
    private readonly ILangNgheDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public GetSubmissionListQueryHandler(ILangNgheDbContext context, IMapper mapper, ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResult<SubmissionDto>>> Handle(GetSubmissionListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SubmissionWorkflows
            .AsNoTracking()
            .Include(x => x.CraftVillage!)
            .ThenInclude(x => x.Region!)
            .AsQueryable();

        if (request.Filter.CraftVillageId.HasValue)
            query = query.Where(x => x.CraftVillageId == request.Filter.CraftVillageId);

        if (request.Filter.Status.HasValue)
            query = query.Where(x => x.CurrentStatus == request.Filter.Status);
            
        if (!string.IsNullOrEmpty(request.Filter.ReportPeriod))
            query = query.Where(x => x.ReportPeriod == request.Filter.ReportPeriod);

        // Ownership filtering
        var dataScope = _currentUser.DataScope;
        var regionId = _currentUser.RegionId;
        if (dataScope == 2) // Tỉnh
        {
            query = query.Where(x => x.CraftVillage!.Region!.ParentId == regionId);
        }
        else if (dataScope == 3) // Xã
        {
            query = query.Where(x => x.CraftVillage!.RegionId == regionId);
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
