using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.PeriodicMetric.Commands;

public record CopyPreviousMetricCommand(
    int CraftVillageId,
    string TargetReportPeriod,
    byte TargetPeriodType
) : IRequest<long>;

public class CopyPreviousMetricCommandHandler : IRequestHandler<CopyPreviousMetricCommand, long>
{
    private readonly ILangNgheDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IVillageOwnershipService _ownershipService;

    public CopyPreviousMetricCommandHandler(ILangNgheDbContext context, IDateTimeProvider dateTimeProvider, IVillageOwnershipService ownershipService)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _ownershipService = ownershipService;
    }

    public async Task<long> Handle(CopyPreviousMetricCommand request, CancellationToken cancellationToken)
    {
        await _ownershipService.ValidateMetricAccessAsync(request.CraftVillageId, cancellationToken);

        var village = await _context.CraftVillages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.CraftVillageId, cancellationToken);
            
        if (village == null)
            throw new NotFoundException("Làng nghề không tồn tại", request.CraftVillageId);

        var exists = await _context.VillagePeriodicMetrics.AnyAsync(
            m => m.CraftVillageId == request.CraftVillageId 
                 && m.ReportPeriod == request.TargetReportPeriod 
                 && m.PeriodType == request.TargetPeriodType,
            cancellationToken);

        if (exists)
            throw new BusinessException("Kỳ báo cáo đích đã tồn tại");

        var source = await _context.VillagePeriodicMetrics
            .AsNoTracking()
            .Where(m => m.CraftVillageId == request.CraftVillageId)
            .OrderByDescending(m => m.ReportPeriod)
            .FirstOrDefaultAsync(cancellationToken);

        if (source == null)
            throw new BusinessException("Không có kỳ báo cáo trước để sao chép");

        var newMetric = new Domain.Entities.VillagePeriodicMetric
        {
            CraftVillageId = request.CraftVillageId,
            CommuneId = source.CommuneId,
            ProvinceId = source.ProvinceId,
            ReportPeriod = request.TargetReportPeriod,
            PeriodType = request.TargetPeriodType,
            BrandProtection = source.BrandProtection,
            HasOCOP3Star = source.HasOCOP3Star,
            ProductionProcess = source.ProductionProcess,
            ExportRate = source.ExportRate,
            ExportCategoryId = source.ExportCategoryId,
            ImportMaterialRate = source.ImportMaterialRate,
            HouseholdCount = source.HouseholdCount,
            EnterpriseCount = source.EnterpriseCount,
            ProductionArea = source.ProductionArea,
            ActiveMonths = source.ActiveMonths,
            Output = source.Output,
            OutputUnitId = source.OutputUnitId,
            Revenue = source.Revenue,
            GrowthRate = source.GrowthRate,
            TotalLabor = source.TotalLabor,
            TrainedLaborRate = source.TrainedLaborRate,
            ArtisanCount = source.ArtisanCount,
            TrainingNeed = source.TrainingNeed,
            AvgIncome = source.AvgIncome,
            DigitalLevel = source.DigitalLevel,
            HasWebsite = source.HasWebsite,
            HasMuseum = source.HasMuseum,
            HasDocArchive = source.HasDocArchive,
            HasWasteCollection = source.HasWasteCollection,
            HasWasteWaterSystem = source.HasWasteWaterSystem,
            HasSolidWasteSystem = source.HasSolidWasteSystem,
            HasEmissionTreatment = source.HasEmissionTreatment,
            HasDedicatedZone = source.HasDedicatedZone,
            HasSupportPolicy = source.HasSupportPolicy,
            SupportFunding = source.SupportFunding,
            HasTourismActivity = source.HasTourismActivity,
            HasTourLinkage = source.HasTourLinkage,
            AnnualVisitors = source.AnnualVisitors,
            DataSource = $"Copy từ kỳ {source.ReportPeriod}",
            ImportedAt = _dateTimeProvider.UtcNow
        };

        _context.VillagePeriodicMetrics.Add(newMetric);
        await _context.SaveChangesAsync(cancellationToken);

        return newMetric.Id;
    }
}

public class CopyPreviousMetricValidator : AbstractValidator<CopyPreviousMetricCommand>
{
    public CopyPreviousMetricValidator()
    {
        RuleFor(x => x.CraftVillageId)
            .GreaterThan(0).WithMessage("CraftVillageId phải lớn hơn 0.");

        RuleFor(x => x.TargetReportPeriod)
            .NotEmpty().WithMessage("Kỳ báo cáo đích không được để trống.")
            .MaximumLength(20).WithMessage("Kỳ báo cáo đích không được vượt quá 20 ký tự.");

        RuleFor(x => x.TargetPeriodType)
            .InclusiveBetween((byte)1, (byte)4).WithMessage("Loại kỳ báo cáo đích không hợp lệ (1: Quý, 2: 6 tháng, 3: Năm).");
    }
}
