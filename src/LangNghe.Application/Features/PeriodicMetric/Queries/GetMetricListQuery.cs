using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Application.Features.PeriodicMetric.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.PeriodicMetric.Queries;

public record GetMetricListQuery(MetricFilterDto Dto) : IRequest<PagedResult<MetricListDto>>;

public class GetMetricListQueryHandler : IRequestHandler<GetMetricListQuery, PagedResult<MetricListDto>>
{
    private readonly ILangNgheDbContext _context;

    public GetMetricListQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<MetricListDto>> Handle(GetMetricListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.VillagePeriodicMetrics
            .AsNoTracking()
            .Include(x => x.CraftVillage)
            .Include(x => x.Commune)
            .Include(x => x.Province)
            .AsQueryable();

        if (request.Dto.CraftVillageId.HasValue)
        {
            query = query.Where(x => x.CraftVillageId == request.Dto.CraftVillageId.Value);
        }

        if (request.Dto.ProvinceId.HasValue)
        {
            query = query.Where(x => x.ProvinceId == request.Dto.ProvinceId.Value);
        }

        if (request.Dto.CommuneId.HasValue)
        {
            query = query.Where(x => x.CommuneId == request.Dto.CommuneId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Dto.ReportPeriod))
        {
            var period = request.Dto.ReportPeriod.Trim();
            query = query.Where(x => x.ReportPeriod == period);
        }

        if (request.Dto.PeriodType.HasValue)
        {
            query = query.Where(x => x.PeriodType == request.Dto.PeriodType.Value);
        }

        if (request.Dto.IsEffective.HasValue)
        {
            query = query.Where(x => x.IsEffective == request.Dto.IsEffective.Value);
        }

        if (request.Dto.YearFrom.HasValue)
        {
            var yearFromStr = request.Dto.YearFrom.Value.ToString();
            query = query.Where(x => string.Compare(x.ReportPeriod, yearFromStr) >= 0);
        }

        if (request.Dto.YearTo.HasValue)
        {
            var yearToStr = request.Dto.YearTo.Value.ToString();
            query = query.Where(x => string.Compare(x.ReportPeriod, yearToStr) <= 0);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.ReportPeriod)
            .ThenBy(x => x.CraftVillageId)
            .Skip((request.Dto.Page - 1) * request.Dto.PageSize)
            .Take(request.Dto.PageSize)
            .Select(x => new MetricListDto(
                x.Id,
                x.CraftVillageId,
                x.CraftVillage != null ? x.CraftVillage.Name : null,
                x.CraftVillage != null ? x.CraftVillage.VillageCode : null,
                x.Commune != null ? x.Commune.Name : null,
                x.Province != null ? x.Province.Name : null,
                x.ReportPeriod,
                x.PeriodType,
                x.Revenue,
                x.TotalLabor,
                x.HouseholdCount,
                x.EnterpriseCount,
                x.IsEffective,
                x.ImportedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<MetricListDto>(items, request.Dto.Page, request.Dto.PageSize, totalCount);
    }
}
