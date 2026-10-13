using System;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Features.PeriodicMetric.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.PeriodicMetric.Commands;

public record CreateMetricCommand(CreateMetricDto Dto) : IRequest<long>;

public class CreateMetricCommandHandler : IRequestHandler<CreateMetricCommand, long>
{
    private readonly ILangNgheDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IVillageOwnershipService _ownershipService;

    public CreateMetricCommandHandler(ILangNgheDbContext context, IDateTimeProvider dateTimeProvider, IVillageOwnershipService ownershipService)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _ownershipService = ownershipService;
    }

    public async Task<long> Handle(CreateMetricCommand request, CancellationToken cancellationToken)
    {
        var village = await _context.CraftVillages
            .Include(x => x.Region)
            .FirstOrDefaultAsync(x => x.Id == request.Dto.CraftVillageId, cancellationToken);
            
        if (village == null)
            throw new NotFoundException("Không tìm thấy làng nghề", request.Dto.CraftVillageId);
            
        await _ownershipService.ValidateMetricAccessAsync(request.Dto.CraftVillageId, cancellationToken);
            
        var communeId = village.RegionId;
        var provinceId = village.Region?.ParentId;
        
        if (provinceId == null)
            throw new BusinessException("Làng nghề không thuộc xã/phường có thông tin tỉnh hợp lệ");

        var exists = await _context.VillagePeriodicMetrics.AnyAsync(
            x => x.CraftVillageId == request.Dto.CraftVillageId 
                 && x.ReportPeriod == request.Dto.ReportPeriod 
                 && x.PeriodType == request.Dto.PeriodType,
            cancellationToken);

        if (exists)
            throw new BusinessException("Kỳ báo cáo này đã tồn tại cho làng nghề");

        var entity = new Domain.Entities.VillagePeriodicMetric
        {
            CraftVillageId = request.Dto.CraftVillageId,
            CommuneId = communeId,
            ProvinceId = provinceId.Value,
            ReportPeriod = request.Dto.ReportPeriod,
            PeriodType = request.Dto.PeriodType,
            BrandProtection = request.Dto.BrandProtection,
            HasOCOP3Star = request.Dto.HasOCOP3Star,
            ProductionProcess = request.Dto.ProductionProcess,
            ExportRate = request.Dto.ExportRate,
            ExportCategoryId = request.Dto.ExportCategoryId,
            ImportMaterialRate = request.Dto.ImportMaterialRate,
            HouseholdCount = request.Dto.HouseholdCount,
            EnterpriseCount = request.Dto.EnterpriseCount,
            ProductionArea = request.Dto.ProductionArea,
            ActiveMonths = request.Dto.ActiveMonths,
            Output = request.Dto.Output,
            OutputUnitId = request.Dto.OutputUnitId,
            Revenue = request.Dto.Revenue,
            GrowthRate = request.Dto.GrowthRate,
            TotalLabor = request.Dto.TotalLabor,
            TrainedLaborRate = request.Dto.TrainedLaborRate,
            ArtisanCount = request.Dto.ArtisanCount,
            TrainingNeed = request.Dto.TrainingNeed,
            AvgIncome = request.Dto.AvgIncome,
            DigitalLevel = request.Dto.DigitalLevel,
            HasWebsite = request.Dto.HasWebsite,
            HasMuseum = request.Dto.HasMuseum,
            HasDocArchive = request.Dto.HasDocArchive,
            HasWasteCollection = request.Dto.HasWasteCollection,
            HasWasteWaterSystem = request.Dto.HasWasteWaterSystem,
            HasSolidWasteSystem = request.Dto.HasSolidWasteSystem,
            HasEmissionTreatment = request.Dto.HasEmissionTreatment,
            HasDedicatedZone = request.Dto.HasDedicatedZone,
            HasSupportPolicy = request.Dto.HasSupportPolicy,
            SupportFunding = request.Dto.SupportFunding,
            HasTourismActivity = request.Dto.HasTourismActivity,
            HasTourLinkage = request.Dto.HasTourLinkage,
            AnnualVisitors = request.Dto.AnnualVisitors,
            DataSource = request.Dto.DataSource,
            ImportedAt = _dateTimeProvider.UtcNow
        };

        _context.VillagePeriodicMetrics.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
