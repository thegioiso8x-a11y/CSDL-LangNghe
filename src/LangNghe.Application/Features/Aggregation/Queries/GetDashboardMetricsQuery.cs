using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Common;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

using LangNghe.Application.Features.Aggregation.Interfaces;

namespace LangNghe.Application.Features.Aggregation.Queries;

public class GetDashboardMetricsQuery : IRequest<ApiResponse<DashboardMetricsDto>>
{
    public string Period { get; set; } = default!;
    public byte PeriodType { get; set; }
    public int RegionId { get; set; }
}

public class DashboardMetricsDto
{
    public VillageTimeSeriesDto? Overall { get; set; }
    public List<IndustryGroupMetricDto> ByIndustryGroup { get; set; } = new();
    public List<VillageTypeMetricDto> ByVillageType { get; set; } = new();
}

public class VillageTimeSeriesDto
{
    public int RegionId { get; set; }
    public byte RegionLevel { get; set; }
    public string Period { get; set; } = default!;
    public byte PeriodType { get; set; }
    
    public int TotalVillages { get; set; }
    public int TraditionalVillages { get; set; }
    public int NewVillages { get; set; }
    public int RestoredVillages { get; set; }
    public int OcopVillages { get; set; }
    public int EffectiveVillages { get; set; }
    
    public int TotalHouseholds { get; set; }
    public int TotalEnterprises { get; set; }
    public long TotalLabor { get; set; }
    public long TrainedLabor { get; set; }
    public int TotalArtisans { get; set; }
    
    public decimal? TotalArea { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalExportValue { get; set; }
    public decimal? AvgRevenuePerVillage { get; set; }
    public decimal? AvgIncome { get; set; }
    public decimal? AvgGrowthRate { get; set; }
    
    public int DigitalizedVillages { get; set; }
    public int EnvCompliantVillages { get; set; }
    public int TourismVillages { get; set; }
    public long TotalVisitors { get; set; }
}

public class IndustryGroupMetricDto
{
    public int IndustryGroupId { get; set; }
    public string IndustryGroupName { get; set; } = default!;
    public int TotalVillages { get; set; }
    public decimal TotalRevenue { get; set; }
    public long TotalLabor { get; set; }
    public decimal TotalExportValue { get; set; }
}

public class VillageTypeMetricDto
{
    public int VillageTypeId { get; set; }
    public string VillageTypeName { get; set; } = default!;
    public int TotalVillages { get; set; }
    public decimal TotalRevenue { get; set; }
    public long TotalLabor { get; set; }
}

public class GetDashboardMetricsQueryHandler : IRequestHandler<GetDashboardMetricsQuery, ApiResponse<DashboardMetricsDto>>
{
    private readonly ILangNgheDbContext _context;
    private readonly IDashboardCacheService _cacheService;
    private readonly ICurrentUserService _currentUser;

    public GetDashboardMetricsQueryHandler(ILangNgheDbContext context, IDashboardCacheService cacheService, ICurrentUserService currentUser)
    {
        _context = context;
        _cacheService = cacheService;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<DashboardMetricsDto>> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.DataScope == 2 || _currentUser.DataScope == 3)
        {
            if (request.RegionId == 0)
            {
                request.RegionId = _currentUser.RegionId ?? 0;
            }
            else if (request.RegionId != _currentUser.RegionId)
            {
                throw new ForbiddenException("Bạn không có quyền xem dữ liệu của vùng này.");
            }
        }

        var cached = await _cacheService.GetAsync(request.Period, request.PeriodType, request.RegionId, cancellationToken);
        if (cached != null)
        {
            return ApiResponse<DashboardMetricsDto>.Ok(cached);
        }

        // Overall
        var overall = await _context.VillageTimeSeries
            .AsNoTracking()
            .Where(x => x.Period == request.Period && x.PeriodType == request.PeriodType && x.RegionId == request.RegionId)
            .Select(x => new VillageTimeSeriesDto
            {
                RegionId = x.RegionId,
                RegionLevel = x.RegionLevel,
                Period = x.Period,
                PeriodType = x.PeriodType,
                TotalVillages = x.TotalVillages,
                TraditionalVillages = x.TraditionalVillages,
                NewVillages = x.NewVillages,
                RestoredVillages = x.RestoredVillages,
                OcopVillages = x.OcopVillages,
                EffectiveVillages = x.EffectiveVillages,
                TotalHouseholds = x.TotalHouseholds,
                TotalEnterprises = x.TotalEnterprises,
                TotalLabor = x.TotalLabor,
                TrainedLabor = x.TrainedLabor,
                TotalArtisans = x.TotalArtisans,
                TotalArea = x.TotalArea,
                TotalRevenue = x.TotalRevenue,
                TotalExportValue = x.TotalExportValue,
                AvgRevenuePerVillage = x.AvgRevenuePerVillage,
                AvgIncome = x.AvgIncome,
                AvgGrowthRate = x.AvgGrowthRate,
                DigitalizedVillages = x.DigitalizedVillages,
                EnvCompliantVillages = x.EnvCompliantVillages,
                TourismVillages = x.TourismVillages,
                TotalVisitors = x.TotalVisitors
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Industry Groups
        var byIndustry = await _context.IndustryGroupAggregates
            .AsNoTracking()
            .Include(x => x.IndustryGroup)
            .Where(x => x.Period == request.Period && x.PeriodType == request.PeriodType && x.RegionId == request.RegionId)
            .Select(x => new IndustryGroupMetricDto
            {
                IndustryGroupId = x.IndustryGroupId,
                IndustryGroupName = x.IndustryGroup != null ? x.IndustryGroup.Name : string.Empty,
                TotalVillages = x.TotalVillages,
                TotalRevenue = x.TotalRevenue,
                TotalLabor = x.TotalLabor,
                TotalExportValue = x.TotalExportValue
            })
            .ToListAsync(cancellationToken);

        // Village Types
        var byType = await _context.VillageTypeAggregates
            .AsNoTracking()
            .Include(x => x.VillageType)
            .Where(x => x.Period == request.Period && x.PeriodType == request.PeriodType && x.RegionId == request.RegionId)
            .Select(x => new VillageTypeMetricDto
            {
                VillageTypeId = x.VillageTypeId,
                VillageTypeName = x.VillageType != null ? x.VillageType.Name : string.Empty,
                TotalVillages = x.TotalVillages,
                TotalRevenue = x.TotalRevenue,
                TotalLabor = x.TotalLabor
            })
            .ToListAsync(cancellationToken);

        var result = new DashboardMetricsDto
        {
            Overall = overall,
            ByIndustryGroup = byIndustry,
            ByVillageType = byType
        };

        await _cacheService.SetAsync(request.Period, request.PeriodType, request.RegionId, result, cancellationToken);

        return ApiResponse<DashboardMetricsDto>.Ok(result);
    }
}
