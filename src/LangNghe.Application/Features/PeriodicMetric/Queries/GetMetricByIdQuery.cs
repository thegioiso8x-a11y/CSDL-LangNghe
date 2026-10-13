using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Features.PeriodicMetric.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.PeriodicMetric.Queries;

public record GetMetricByIdQuery(long Id) : IRequest<MetricDto>;

public class GetMetricByIdQueryHandler : IRequestHandler<GetMetricByIdQuery, MetricDto>
{
    private readonly ILangNgheDbContext _context;

    public GetMetricByIdQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<MetricDto> Handle(GetMetricByIdQuery request, CancellationToken cancellationToken)
    {
        var metric = await _context.VillagePeriodicMetrics
            .AsNoTracking()
            .Include(x => x.CraftVillage)
            .Include(x => x.Commune)
            .Include(x => x.Province)
            .Include(x => x.ExportCategory)
            .Include(x => x.OutputUnit)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (metric == null)
            throw new NotFoundException("Không tìm thấy chỉ tiêu kỳ báo cáo", request.Id);

        return new MetricDto(
            metric.Id,
            metric.CraftVillageId,
            metric.CraftVillage?.Name,
            metric.CommuneId,
            metric.Commune?.Name,
            metric.ProvinceId,
            metric.Province?.Name,
            metric.ReportPeriod,
            metric.PeriodType,
            metric.ImportBatchId,
            metric.BrandProtection,
            metric.HasOCOP3Star,
            metric.ProductionProcess,
            metric.ExportRate,
            metric.ExportCategoryId,
            metric.ExportCategory?.Name,
            metric.ImportMaterialRate,
            metric.HouseholdCount,
            metric.EnterpriseCount,
            metric.ProductionArea,
            metric.ActiveMonths,
            metric.Output,
            metric.OutputUnitId,
            metric.OutputUnit?.Name,
            metric.Revenue,
            metric.GrowthRate,
            metric.TotalLabor,
            metric.TrainedLaborRate,
            metric.ArtisanCount,
            metric.TrainingNeed,
            metric.AvgIncome,
            metric.DigitalLevel,
            metric.HasWebsite,
            metric.HasMuseum,
            metric.HasDocArchive,
            metric.HasWasteCollection,
            metric.HasWasteWaterSystem,
            metric.HasSolidWasteSystem,
            metric.HasEmissionTreatment,
            metric.HasDedicatedZone,
            metric.HasSupportPolicy,
            metric.SupportFunding,
            metric.HasTourismActivity,
            metric.HasTourLinkage,
            metric.AnnualVisitors,
            metric.ExportValueVND,
            metric.IsEffective,
            metric.ImportedAt,
            metric.DataSource
        );
    }
}
