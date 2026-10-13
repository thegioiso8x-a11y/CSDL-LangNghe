using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Features.PeriodicMetric.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.PeriodicMetric.Queries;

public record GetMetricsByVillageQuery(int VillageId) : IRequest<List<MetricDto>>;

public class GetMetricsByVillageQueryHandler : IRequestHandler<GetMetricsByVillageQuery, List<MetricDto>>
{
    private readonly ILangNgheDbContext _context;

    public GetMetricsByVillageQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<List<MetricDto>> Handle(GetMetricsByVillageQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.VillagePeriodicMetrics
            .AsNoTracking()
            .Include(x => x.CraftVillage)
            .Include(x => x.Commune)
            .Include(x => x.Province)
            .Include(x => x.ExportCategory)
            .Include(x => x.OutputUnit)
            .Where(x => x.CraftVillageId == request.VillageId)
            .OrderByDescending(x => x.ReportPeriod)
            .Select(x => new MetricDto(
                x.Id,
                x.CraftVillageId,
                x.CraftVillage != null ? x.CraftVillage.Name : null,
                x.CommuneId,
                x.Commune != null ? x.Commune.Name : null,
                x.ProvinceId,
                x.Province != null ? x.Province.Name : null,
                x.ReportPeriod,
                x.PeriodType,
                x.ImportBatchId,
                x.BrandProtection,
                x.HasOCOP3Star,
                x.ProductionProcess,
                x.ExportRate,
                x.ExportCategoryId,
                x.ExportCategory != null ? x.ExportCategory.Name : null,
                x.ImportMaterialRate,
                x.HouseholdCount,
                x.EnterpriseCount,
                x.ProductionArea,
                x.ActiveMonths,
                x.Output,
                x.OutputUnitId,
                x.OutputUnit != null ? x.OutputUnit.Name : null,
                x.Revenue,
                x.GrowthRate,
                x.TotalLabor,
                x.TrainedLaborRate,
                x.ArtisanCount,
                x.TrainingNeed,
                x.AvgIncome,
                x.DigitalLevel,
                x.HasWebsite,
                x.HasMuseum,
                x.HasDocArchive,
                x.HasWasteCollection,
                x.HasWasteWaterSystem,
                x.HasSolidWasteSystem,
                x.HasEmissionTreatment,
                x.HasDedicatedZone,
                x.HasSupportPolicy,
                x.SupportFunding,
                x.HasTourismActivity,
                x.HasTourLinkage,
                x.AnnualVisitors,
                x.ExportValueVND,
                x.IsEffective,
                x.ImportedAt,
                x.DataSource
            ))
            .ToListAsync(cancellationToken);

        return items;
    }
}
