namespace LangNghe.Application.Features.Aggregation.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LangNghe.Application.Features.Aggregation.Interfaces;
using LangNghe.Domain.Entities;
using LangNghe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class AggRebuildService : IAggRebuildService
{
    private readonly ILangNgheDbContext _context;
    private readonly IDashboardCacheService _cacheService;

    public AggRebuildService(ILangNgheDbContext context, IDashboardCacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task RebuildAllAsync(string period, byte periodType, CancellationToken ct)
    {
        await RebuildTimeSeriesAsync(period, periodType, ct);
        await RebuildIndustryGroupsAsync(period, periodType, ct);
        await RebuildVillageTypesAsync(period, periodType, ct);
    }

    public async Task RebuildTimeSeriesAsync(string period, byte periodType, CancellationToken ct)
    {
        var metrics = await _context.VillagePeriodicMetrics
            .AsNoTracking()
            .Include(m => m.CraftVillage)
                .ThenInclude(v => v!.VillageType)
            .Where(m => m.ReportPeriod == period && m.PeriodType == periodType)
            .ToListAsync(ct);

        if (!metrics.Any()) return;

        var contributions = new List<(int RegionId, byte RegionLevel, int? ParentRegionId, VillagePeriodicMetric Metric)>();
        foreach (var m in metrics)
        {
            contributions.Add((m.ProvinceId, 1, null, m));
            contributions.Add((m.CommuneId, 2, m.ProvinceId, m));
        }

        var grouped = contributions.GroupBy(x => new { x.RegionId, x.RegionLevel, x.ParentRegionId });
        var now = DateTime.UtcNow;

        var existingAggs = await _context.VillageTimeSeries
            .AsTracking()
            .Where(x => x.Period == period && x.PeriodType == periodType)
            .ToDictionaryAsync(x => x.RegionId, ct);

        foreach (var g in grouped)
        {
            var mList = g.Select(x => x.Metric).ToList();
            var totalVillages = mList.Select(x => x.CraftVillageId).Distinct().Count();
            
            var totalRevenue = mList.Sum(x => x.Revenue);
            var totalLabor = mList.Sum(x => (long)x.TotalLabor);

            var newAgg = new VillageTimeSeries
            {
                RegionId = g.Key.RegionId,
                RegionLevel = g.Key.RegionLevel,
                ParentRegionId = g.Key.ParentRegionId,
                Period = period,
                PeriodType = periodType,
                TotalVillages = totalVillages,
                TraditionalVillages = mList.Count(x => x.CraftVillage?.VillageType?.Code == "LNTT"),
                NewVillages = mList.Count(x => x.CraftVillage?.VillageType?.Code == "LNM"),
                RestoredVillages = mList.Count(x => x.CraftVillage?.NeedRestoration == true || x.CraftVillage?.VillageType?.IsRestored == true),
                OcopVillages = mList.Count(x => x.HasOCOP3Star),
                EffectiveVillages = mList.Count(x => (x.ActiveMonths ?? 0) >= 9 && x.Revenue > 0),
                TotalHouseholds = mList.Sum(x => x.HouseholdCount),
                TotalEnterprises = mList.Sum(x => x.EnterpriseCount),
                TotalLabor = totalLabor,
                TrainedLabor = mList.Sum(x => (long)(x.TotalLabor * (x.TrainedLaborRate ?? 0) / 100)),
                TotalArtisans = mList.Sum(x => x.ArtisanCount),
                TotalArea = mList.Sum(x => x.ProductionArea ?? 0m),
                TotalRevenue = totalRevenue,
                TotalExportValue = mList.Sum(x => x.Revenue * (x.ExportRate / 100)),
                AvgRevenuePerVillage = totalVillages > 0 ? totalRevenue / totalVillages : 0,
                AvgIncome = mList.Any(x => x.AvgIncome.HasValue) ? mList.Where(x => x.AvgIncome.HasValue).Average(x => x.AvgIncome) : null,
                AvgGrowthRate = mList.Any(x => x.GrowthRate.HasValue) ? mList.Where(x => x.GrowthRate.HasValue).Average(x => x.GrowthRate) : null,
                DigitalizedVillages = mList.Count(x => (x.DigitalLevel ?? 0) > 0 || x.HasWebsite),
                EnvCompliantVillages = mList.Count(x => x.HasWasteCollection && x.HasWasteWaterSystem && x.HasSolidWasteSystem),
                TourismVillages = mList.Count(x => x.HasTourismActivity),
                TotalVisitors = mList.Sum(x => x.AnnualVisitors ?? 0L),
                LastUpdated = now,
                CalculatedBy = "Manual"
            };

            if (existingAggs.TryGetValue(g.Key.RegionId, out var existing))
            {
                existing.RegionLevel = newAgg.RegionLevel;
                existing.ParentRegionId = newAgg.ParentRegionId;
                existing.TotalVillages = newAgg.TotalVillages;
                existing.TraditionalVillages = newAgg.TraditionalVillages;
                existing.NewVillages = newAgg.NewVillages;
                existing.RestoredVillages = newAgg.RestoredVillages;
                existing.OcopVillages = newAgg.OcopVillages;
                existing.EffectiveVillages = newAgg.EffectiveVillages;
                existing.TotalHouseholds = newAgg.TotalHouseholds;
                existing.TotalEnterprises = newAgg.TotalEnterprises;
                existing.TotalLabor = newAgg.TotalLabor;
                existing.TrainedLabor = newAgg.TrainedLabor;
                existing.TotalArtisans = newAgg.TotalArtisans;
                existing.TotalArea = newAgg.TotalArea;
                existing.TotalRevenue = newAgg.TotalRevenue;
                existing.TotalExportValue = newAgg.TotalExportValue;
                existing.AvgRevenuePerVillage = newAgg.AvgRevenuePerVillage;
                existing.AvgIncome = newAgg.AvgIncome;
                existing.AvgGrowthRate = newAgg.AvgGrowthRate;
                existing.DigitalizedVillages = newAgg.DigitalizedVillages;
                existing.EnvCompliantVillages = newAgg.EnvCompliantVillages;
                existing.TourismVillages = newAgg.TourismVillages;
                existing.TotalVisitors = newAgg.TotalVisitors;
                existing.LastUpdated = newAgg.LastUpdated;
                existing.CalculatedBy = newAgg.CalculatedBy;
            }
            else
            {
                _context.VillageTimeSeries.Add(newAgg);
            }
        }

        await _context.SaveChangesAsync(ct);

        foreach (var rId in grouped.Select(x => x.Key.RegionId).Distinct())
        {
            _cacheService.Invalidate(period, periodType, rId);
        }
    }

    public async Task RebuildIndustryGroupsAsync(string period, byte periodType, CancellationToken ct)
    {
        var metrics = await _context.VillagePeriodicMetrics
            .AsNoTracking()
            .Include(m => m.CraftVillage)
            .Where(m => m.ReportPeriod == period && m.PeriodType == periodType && m.CraftVillage != null && m.CraftVillage.IndustryGroupId.HasValue)
            .ToListAsync(ct);

        if (!metrics.Any()) return;

        var contributions = new List<(int IndustryGroupId, int RegionId, byte RegionLevel, VillagePeriodicMetric Metric)>();
        foreach (var m in metrics)
        {
            var indId = m.CraftVillage!.IndustryGroupId!.Value;
            contributions.Add((indId, 0, 0, m)); // National
            contributions.Add((indId, m.ProvinceId, 1, m)); // Province
            contributions.Add((indId, m.CommuneId, 2, m)); // Commune
        }

        var grouped = contributions.GroupBy(x => new { x.IndustryGroupId, x.RegionId, x.RegionLevel });
        var now = DateTime.UtcNow;

        var existingAggs = await _context.IndustryGroupAggregates
            .AsTracking()
            .Where(x => x.Period == period && x.PeriodType == periodType)
            .ToDictionaryAsync(x => $"{x.IndustryGroupId}_{x.RegionId}", ct);

        foreach (var g in grouped)
        {
            var mList = g.Select(x => x.Metric).ToList();
            var newAgg = new IndustryGroupAggregate
            {
                IndustryGroupId = g.Key.IndustryGroupId,
                RegionId = g.Key.RegionId,
                RegionLevel = g.Key.RegionLevel,
                Period = period,
                PeriodType = periodType,
                TotalVillages = mList.Select(x => x.CraftVillageId).Distinct().Count(),
                TotalRevenue = mList.Sum(x => x.Revenue),
                TotalLabor = mList.Sum(x => (long)x.TotalLabor),
                TotalExportValue = mList.Sum(x => x.Revenue * (x.ExportRate / 100)),
                LastUpdated = now
            };

            var key = $"{g.Key.IndustryGroupId}_{g.Key.RegionId}";
            if (existingAggs.TryGetValue(key, out var existing))
            {
                existing.RegionLevel = newAgg.RegionLevel;
                existing.TotalVillages = newAgg.TotalVillages;
                existing.TotalRevenue = newAgg.TotalRevenue;
                existing.TotalLabor = newAgg.TotalLabor;
                existing.TotalExportValue = newAgg.TotalExportValue;
                existing.LastUpdated = newAgg.LastUpdated;
            }
            else
            {
                _context.IndustryGroupAggregates.Add(newAgg);
            }
        }

        await _context.SaveChangesAsync(ct);

        foreach (var rId in grouped.Select(x => x.Key.RegionId).Distinct())
        {
            _cacheService.Invalidate(period, periodType, rId);
        }
    }

    public async Task RebuildVillageTypesAsync(string period, byte periodType, CancellationToken ct)
    {
        var metrics = await _context.VillagePeriodicMetrics
            .AsNoTracking()
            .Include(m => m.CraftVillage)
            .Where(m => m.ReportPeriod == period && m.PeriodType == periodType && m.CraftVillage != null && m.CraftVillage.VillageTypeId.HasValue)
            .ToListAsync(ct);

        if (!metrics.Any()) return;

        var contributions = new List<(int VillageTypeId, int RegionId, byte RegionLevel, VillagePeriodicMetric Metric)>();
        foreach (var m in metrics)
        {
            var vtId = m.CraftVillage!.VillageTypeId!.Value;
            contributions.Add((vtId, 0, 0, m)); // National
            contributions.Add((vtId, m.ProvinceId, 1, m)); // Province
            contributions.Add((vtId, m.CommuneId, 2, m)); // Commune
        }

        var grouped = contributions.GroupBy(x => new { x.VillageTypeId, x.RegionId, x.RegionLevel });
        var now = DateTime.UtcNow;

        var existingAggs = await _context.VillageTypeAggregates
            .AsTracking()
            .Where(x => x.Period == period && x.PeriodType == periodType)
            .ToDictionaryAsync(x => $"{x.VillageTypeId}_{x.RegionId}", ct);

        foreach (var g in grouped)
        {
            var mList = g.Select(x => x.Metric).ToList();
            var newAgg = new VillageTypeAggregate
            {
                VillageTypeId = g.Key.VillageTypeId,
                RegionId = g.Key.RegionId,
                RegionLevel = g.Key.RegionLevel,
                Period = period,
                PeriodType = periodType,
                TotalVillages = mList.Select(x => x.CraftVillageId).Distinct().Count(),
                TotalRevenue = mList.Sum(x => x.Revenue),
                TotalLabor = mList.Sum(x => (long)x.TotalLabor),
                LastUpdated = now
            };

            var key = $"{g.Key.VillageTypeId}_{g.Key.RegionId}";
            if (existingAggs.TryGetValue(key, out var existing))
            {
                existing.RegionLevel = newAgg.RegionLevel;
                existing.TotalVillages = newAgg.TotalVillages;
                existing.TotalRevenue = newAgg.TotalRevenue;
                existing.TotalLabor = newAgg.TotalLabor;
                existing.LastUpdated = newAgg.LastUpdated;
            }
            else
            {
                _context.VillageTypeAggregates.Add(newAgg);
            }
        }

        await _context.SaveChangesAsync(ct);

        foreach (var rId in grouped.Select(x => x.Key.RegionId).Distinct())
        {
            _cacheService.Invalidate(period, periodType, rId);
        }
    }
}
