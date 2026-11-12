using System;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Features.PeriodicMetric.Dtos;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.PeriodicMetric.Commands;

public record UpdateMetricCommand(long Id, UpdateMetricDto Dto) : IRequest<bool>;

public class UpdateMetricCommandHandler : IRequestHandler<UpdateMetricCommand, bool>
{
    private readonly ILangNgheDbContext _context;
    private readonly IVillageOwnershipService _ownershipService;
    public UpdateMetricCommandHandler(ILangNgheDbContext context, IVillageOwnershipService ownershipService)
    {
        _context = context;
        _ownershipService = ownershipService;
    }

    public async Task<bool> Handle(UpdateMetricCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.VillagePeriodicMetrics
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            
        if (entity == null)
            throw new NotFoundException("Không tìm thấy chỉ tiêu định kỳ", request.Id);
            
        await _ownershipService.ValidateMetricAccessAsync(entity.CraftVillageId, cancellationToken);
            
        entity.CraftVillageId = request.Dto.CraftVillageId;
        entity.ReportPeriod = request.Dto.ReportPeriod;
        entity.PeriodType = request.Dto.PeriodType;
        entity.BrandProtection = request.Dto.BrandProtection;
        entity.HasOCOP3Star = request.Dto.HasOCOP3Star;
        entity.ProductionProcess = request.Dto.ProductionProcess;
        entity.ExportRate = request.Dto.ExportRate;
        entity.ExportCategoryId = request.Dto.ExportCategoryId;
        entity.ImportMaterialRate = request.Dto.ImportMaterialRate;
        entity.HouseholdCount = request.Dto.HouseholdCount;
        entity.EnterpriseCount = request.Dto.EnterpriseCount;
        entity.ProductionArea = request.Dto.ProductionArea;
            // Kỳ Năm (PeriodType = 3): nếu không nhập số tháng hoạt động thì mặc định 12 tháng
        entity.ActiveMonths = request.Dto.PeriodType == 3
            ? (request.Dto.ActiveMonths ?? 12)
            : request.Dto.ActiveMonths;
        entity.Output = request.Dto.Output;
        entity.OutputUnitId = request.Dto.OutputUnitId;
        entity.Revenue = request.Dto.Revenue;
        entity.GrowthRate = request.Dto.GrowthRate;
        entity.TotalLabor = request.Dto.TotalLabor;
        entity.TrainedLaborRate = request.Dto.TrainedLaborRate;
        entity.ArtisanCount = request.Dto.ArtisanCount;
        entity.TrainingNeed = request.Dto.TrainingNeed;
        entity.AvgIncome = request.Dto.AvgIncome;
        entity.DigitalLevel = request.Dto.DigitalLevel;
        entity.HasWebsite = request.Dto.HasWebsite;
        entity.HasMuseum = request.Dto.HasMuseum;
        entity.HasDocArchive = request.Dto.HasDocArchive;
        entity.HasWasteCollection = request.Dto.HasWasteCollection;
        entity.HasWasteWaterSystem = request.Dto.HasWasteWaterSystem;
        entity.HasSolidWasteSystem = request.Dto.HasSolidWasteSystem;
        entity.HasEmissionTreatment = request.Dto.HasEmissionTreatment;
        entity.HasDedicatedZone = request.Dto.HasDedicatedZone;
        entity.HasSupportPolicy = request.Dto.HasSupportPolicy;
        entity.SupportFunding = request.Dto.SupportFunding;
        entity.HasTourismActivity = request.Dto.HasTourismActivity;
        entity.HasTourLinkage = request.Dto.HasTourLinkage;
        entity.AnnualVisitors = request.Dto.AnnualVisitors;
        entity.DataSource = request.Dto.DataSource;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

