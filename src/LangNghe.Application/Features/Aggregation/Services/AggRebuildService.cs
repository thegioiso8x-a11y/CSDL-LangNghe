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

        // 1. Xóa dữ liệu cũ của kỳ này để tránh trùng lặp
        var existingAggs = await _context.VillageTimeSeries
            .AsTracking()
            .Where(x => x.Period == period && x.PeriodType == periodType)
            .ToListAsync(ct);
        
        _context.VillageTimeSeries.RemoveRange(existingAggs);
        await _context.SaveChangesAsync(ct);

        var contributions = new List<(int RegionId, byte RegionLevel, int? ParentRegionId, VillagePeriodicMetric Metric)>();
        foreach (var m in metrics)
        {
            // Bỏ qua nếu dữ liệu tỉnh/xã bị lỗi
            if (m.ProvinceId <= 0) continue; 

            contributions.Add((m.ProvinceId, 1, null, m));
            if (m.CommuneId > 0)
            {
                contributions.Add((m.CommuneId, 2, m.ProvinceId, m));
            }
        }

        var grouped = contributions.GroupBy(x => new { x.RegionId, x.RegionLevel, x.ParentRegionId });
        var now = DateTime.UtcNow;

        // 2. Lưu dữ liệu cấp Tỉnh và Xã
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

            _context.VillageTimeSeries.Add(newAgg);
        }

        // 3. Tính toán trực tiếp bản ghi Toàn quốc (RegionId = 0) từ danh sách metrics gốc
        var totalVillagesNat = metrics.Select(x => x.CraftVillageId).Distinct().Count();
        var totalRevenueNat = metrics.Sum(x => x.Revenue);
        var totalLaborNat = metrics.Sum(x => (long)x.TotalLabor);

        var nationalAggregate = new VillageTimeSeries
        {
            RegionId = 0,
            RegionLevel = 0,
            ParentRegionId = null,
            Period = period,
            PeriodType = periodType,
            TotalVillages = totalVillagesNat,
            TraditionalVillages = metrics.Count(x => x.CraftVillage?.VillageType?.Code == "LNTT"),
            NewVillages = metrics.Count(x => x.CraftVillage?.VillageType?.Code == "LNM"),
            RestoredVillages = metrics.Count(x => x.CraftVillage?.NeedRestoration == true || x.CraftVillage?.VillageType?.IsRestored == true),
            OcopVillages = metrics.Count(x => x.HasOCOP3Star),
            EffectiveVillages = metrics.Count(x => (x.ActiveMonths ?? 0) >= 9 && x.Revenue > 0),
            TotalHouseholds = metrics.Sum(x => x.HouseholdCount),
            TotalEnterprises = metrics.Sum(x => x.EnterpriseCount),
            TotalLabor = totalLaborNat,
            TrainedLabor = metrics.Sum(x => (long)(x.TotalLabor * (x.TrainedLaborRate ?? 0) / 100)),
            TotalArtisans = metrics.Sum(x => x.ArtisanCount),
            TotalArea = metrics.Sum(x => x.ProductionArea ?? 0m),
            TotalRevenue = totalRevenueNat,
            TotalExportValue = metrics.Sum(x => x.Revenue * (x.ExportRate / 100)),
            AvgRevenuePerVillage = totalVillagesNat > 0 ? totalRevenueNat / totalVillagesNat : 0,
            AvgIncome = metrics.Any(x => x.AvgIncome.HasValue) ? metrics.Where(x => x.AvgIncome.HasValue).Average(x => x.AvgIncome) : null,
            AvgGrowthRate = metrics.Any(x => x.GrowthRate.HasValue) ? metrics.Where(x => x.GrowthRate.HasValue).Average(x => x.GrowthRate) : null,
            DigitalizedVillages = metrics.Count(x => (x.DigitalLevel ?? 0) > 0 || x.HasWebsite),
            EnvCompliantVillages = metrics.Count(x => x.HasWasteCollection && x.HasWasteWaterSystem && x.HasSolidWasteSystem),
            TourismVillages = metrics.Count(x => x.HasTourismActivity),
            TotalVisitors = metrics.Sum(x => x.AnnualVisitors ?? 0L),
            LastUpdated = now,
            CalculatedBy = "Manual-National"
        };

        _context.VillageTimeSeries.Add(nationalAggregate);

        await _context.SaveChangesAsync(ct);

        _cacheService.Invalidate(period, periodType, 0);
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

        var existingAggs = await _context.IndustryGroupAggregates
            .AsTracking()
            .Where(x => x.Period == period && x.PeriodType == periodType)
            .ToListAsync(ct);
        
        _context.IndustryGroupAggregates.RemoveRange(existingAggs);
        await _context.SaveChangesAsync(ct);

        var contributions = new List<(int IndustryGroupId, int RegionId, byte RegionLevel, VillagePeriodicMetric Metric)>();
        foreach (var m in metrics)
        {
            var indId = m.CraftVillage!.IndustryGroupId!.Value;
            contributions.Add((indId, 0, 0, m)); // National
            if (m.ProvinceId > 0) contributions.Add((indId, m.ProvinceId, 1, m)); // Province
            if (m.CommuneId > 0) contributions.Add((indId, m.CommuneId, 2, m)); // Commune
        }

        var grouped = contributions.GroupBy(x => new { x.IndustryGroupId, x.RegionId, x.RegionLevel });
        var now = DateTime.UtcNow;

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
            _context.IndustryGroupAggregates.Add(newAgg);
        }

        await _context.SaveChangesAsync(ct);
        _cacheService.Invalidate(period, periodType, 0);
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

        var existingAggs = await _context.VillageTypeAggregates
            .AsTracking()
            .Where(x => x.Period == period && x.PeriodType == periodType)
            .ToListAsync(ct);
            
        _context.VillageTypeAggregates.RemoveRange(existingAggs);
        await _context.SaveChangesAsync(ct);

        var contributions = new List<(int VillageTypeId, int RegionId, byte RegionLevel, VillagePeriodicMetric Metric)>();
        foreach (var m in metrics)
        {
            var vtId = m.CraftVillage!.VillageTypeId!.Value;
            contributions.Add((vtId, 0, 0, m)); // National
            if (m.ProvinceId > 0) contributions.Add((vtId, m.ProvinceId, 1, m)); // Province
            if (m.CommuneId > 0) contributions.Add((vtId, m.CommuneId, 2, m)); // Commune
        }

        var grouped = contributions.GroupBy(x => new { x.VillageTypeId, x.RegionId, x.RegionLevel });
        var now = DateTime.UtcNow;

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
            _context.VillageTypeAggregates.Add(newAgg);
        }

        await _context.SaveChangesAsync(ct);
        _cacheService.Invalidate(period, periodType, 0);
        foreach (var rId in grouped.Select(x => x.Key.RegionId).Distinct())
        {
            _cacheService.Invalidate(period, periodType, rId);
        }
    }
}