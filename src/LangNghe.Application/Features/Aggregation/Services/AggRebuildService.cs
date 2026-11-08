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

    private const byte YearlyPeriodType = 3;

    public async Task RebuildAllAsync(string period, byte periodType, CancellationToken ct)
    {
        await RebuildTimeSeriesAsync(period, periodType, ct);
        await RebuildIndustryGroupsAsync(period, periodType, ct);
        await RebuildVillageTypesAsync(period, periodType, ct);
    }

    public async Task RebuildAllPeriodsAsync(CancellationToken ct)
    {
        var periods = await _context.VillagePeriodicMetrics
            .AsNoTracking()
            .Select(m => new { m.ReportPeriod, m.PeriodType })
            .Distinct()
            .ToListAsync(ct);

        // 1. Dọn sạch toàn bộ bảng Agg
        var allTimeSeries = await _context.VillageTimeSeries.AsTracking().ToListAsync(ct);
        _context.VillageTimeSeries.RemoveRange(allTimeSeries);

        var allIndustryGroups = await _context.IndustryGroupAggregates.AsTracking().ToListAsync(ct);
        _context.IndustryGroupAggregates.RemoveRange(allIndustryGroups);

        var allVillageTypes = await _context.VillageTypeAggregates.AsTracking().ToListAsync(ct);
        _context.VillageTypeAggregates.RemoveRange(allVillageTypes);

        await _context.SaveChangesAsync(ct);

        // 2. Rebuild lại từng kỳ gốc
        foreach (var p in periods)
        {
            await RebuildTimeSeriesAsync(p.ReportPeriod, p.PeriodType, ct);
            await RebuildIndustryGroupsAsync(p.ReportPeriod, p.PeriodType, ct);
            await RebuildVillageTypesAsync(p.ReportPeriod, p.PeriodType, ct);
        }

        // 3. Roll-up theo NĂM (PeriodType = 3)
        var years = periods.Select(p => p.ReportPeriod).Distinct().ToList();
        foreach (var year in years)
        {
            await RebuildYearlyRollupAsync(year, ct);
        }
    }

    /// <summary>
    /// Roll-up dữ liệu của một năm lên bản ghi Agg PeriodType = 3.
    /// Xử lý triệt để việc gộp dữ liệu Tháng và Năm của các làng nghề khác nhau.
    /// </summary>
    private async Task RebuildYearlyRollupAsync(string year, CancellationToken ct)
    {
        // Lấy tất cả metrics của năm (cả Tháng và Năm) — KHÔNG lọc IsEffective
        var allMetrics = await _context.VillagePeriodicMetrics
            .AsNoTracking()
            .Include(m => m.CraftVillage)
                .ThenInclude(v => v!.VillageType)
            .Where(m => m.ReportPeriod == year)
            .ToListAsync(ct);

        if (!allMetrics.Any()) return;

        // Lọc trùng: 
        // Nếu làng nghề đã báo cáo theo Năm (PeriodType=3), ta lấy báo cáo Năm đó.
        // Nếu làng nghề chỉ báo cáo theo Tháng/Quý, ta gom hết lại để SUM.
        var villagesWithYearly = allMetrics.Where(m => m.PeriodType == YearlyPeriodType).Select(m => m.CraftVillageId).ToHashSet();
        var metricsForYear = allMetrics.Where(m => 
            (villagesWithYearly.Contains(m.CraftVillageId) && m.PeriodType == YearlyPeriodType) || 
            (!villagesWithYearly.Contains(m.CraftVillageId))
        ).ToList();

        // Xóa Agg cũ của năm (nếu có)
        var existingTs = await _context.VillageTimeSeries.AsTracking().Where(x => x.Period == year && x.PeriodType == YearlyPeriodType).ToListAsync(ct);
        _context.VillageTimeSeries.RemoveRange(existingTs);

        var existingIg = await _context.IndustryGroupAggregates.AsTracking().Where(x => x.Period == year && x.PeriodType == YearlyPeriodType).ToListAsync(ct);
        _context.IndustryGroupAggregates.RemoveRange(existingIg);

        var existingVt = await _context.VillageTypeAggregates.AsTracking().Where(x => x.Period == year && x.PeriodType == YearlyPeriodType).ToListAsync(ct);
        _context.VillageTypeAggregates.RemoveRange(existingVt);

        await _context.SaveChangesAsync(ct);

        // Build và Insert dữ liệu mới (Gọi lại 3 hàm Builder dùng chung)
        var tsAggs = BuildTimeSeriesAggregates(metricsForYear, year, YearlyPeriodType);
        _context.VillageTimeSeries.AddRange(tsAggs);

        var igAggs = BuildIndustryGroupAggregates(metricsForYear, year, YearlyPeriodType);
        _context.IndustryGroupAggregates.AddRange(igAggs);

        var vtAggs = BuildVillageTypeAggregates(metricsForYear, year, YearlyPeriodType);
        _context.VillageTypeAggregates.AddRange(vtAggs);

        await _context.SaveChangesAsync(ct);

        // Xóa cache
        _cacheService.Invalidate(year, YearlyPeriodType, 0);
        foreach (var rId in tsAggs.Select(x => x.RegionId).Distinct())
        {
            _cacheService.Invalidate(year, YearlyPeriodType, rId);
        }
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

        var existingAggs = await _context.VillageTimeSeries.AsTracking().Where(x => x.Period == period && x.PeriodType == periodType).ToListAsync(ct);
        _context.VillageTimeSeries.RemoveRange(existingAggs);
        await _context.SaveChangesAsync(ct);

        var aggregates = BuildTimeSeriesAggregates(metrics, period, periodType);
        _context.VillageTimeSeries.AddRange(aggregates);
        await _context.SaveChangesAsync(ct);

        _cacheService.Invalidate(period, periodType, 0);
        foreach (var rId in aggregates.Select(x => x.RegionId).Distinct())
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

        var existingAggs = await _context.IndustryGroupAggregates.AsTracking().Where(x => x.Period == period && x.PeriodType == periodType).ToListAsync(ct);
        _context.IndustryGroupAggregates.RemoveRange(existingAggs);
        await _context.SaveChangesAsync(ct);

        var newAggs = BuildIndustryGroupAggregates(metrics, period, periodType);
        _context.IndustryGroupAggregates.AddRange(newAggs);
        await _context.SaveChangesAsync(ct);

        _cacheService.Invalidate(period, periodType, 0);
        foreach (var rId in newAggs.Select(x => x.RegionId).Distinct())
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

        var existingAggs = await _context.VillageTypeAggregates.AsTracking().Where(x => x.Period == period && x.PeriodType == periodType).ToListAsync(ct);
        _context.VillageTypeAggregates.RemoveRange(existingAggs);
        await _context.SaveChangesAsync(ct);

        var newAggs = BuildVillageTypeAggregates(metrics, period, periodType);
        _context.VillageTypeAggregates.AddRange(newAggs);
        await _context.SaveChangesAsync(ct);

        _cacheService.Invalidate(period, periodType, 0);
        foreach (var rId in newAggs.Select(x => x.RegionId).Distinct())
        {
            _cacheService.Invalidate(period, periodType, rId);
        }
    }

    // ==========================================
    // CÁC HÀM BUILDER DÙNG CHUNG (TÁCH RA ĐỂ TÁI SỬ DỤNG)
    // ==========================================

    private static List<VillageTimeSeries> BuildTimeSeriesAggregates(List<VillagePeriodicMetric> metrics, string period, byte periodType)
    {
        var result = new List<VillageTimeSeries>();
        var now = DateTime.UtcNow;

        var contributions = new List<(int RegionId, byte RegionLevel, int? ParentRegionId, VillagePeriodicMetric Metric)>();
        foreach (var m in metrics)
        {
            if (m.ProvinceId <= 0) continue; 
            contributions.Add((m.ProvinceId, 1, null, m));
            if (m.CommuneId > 0) contributions.Add((m.CommuneId, 2, m.ProvinceId, m));
        }

        var grouped = contributions.GroupBy(x => new { x.RegionId, x.RegionLevel, x.ParentRegionId });

        foreach (var g in grouped)
        {
            var mList = g.Select(x => x.Metric).ToList();
            var totalVillages = mList.Select(x => x.CraftVillageId).Distinct().Count();
            var totalRevenue = mList.Sum(x => x.Revenue);
            var totalLabor = mList.Sum(x => (long)x.TotalLabor);

            result.Add(new VillageTimeSeries
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
            });
        }

        // Toàn quốc
        var totalVillagesNat = metrics.Select(x => x.CraftVillageId).Distinct().Count();
        var totalRevenueNat = metrics.Sum(x => x.Revenue);
        var totalLaborNat = metrics.Sum(x => (long)x.TotalLabor);

        result.Add(new VillageTimeSeries
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
        });

        return result;
    }

    private static List<IndustryGroupAggregate> BuildIndustryGroupAggregates(List<VillagePeriodicMetric> metrics, string period, byte periodType)
    {
        var result = new List<IndustryGroupAggregate>();
        var contributions = new List<(int IndustryGroupId, int RegionId, byte RegionLevel, VillagePeriodicMetric Metric)>();
        
        foreach (var m in metrics)
        {
            if (m.CraftVillage?.IndustryGroupId == null) continue;
            var indId = m.CraftVillage.IndustryGroupId.Value;
            contributions.Add((indId, 0, 0, m)); 
            if (m.ProvinceId > 0) contributions.Add((indId, m.ProvinceId, 1, m)); 
            if (m.CommuneId > 0) contributions.Add((indId, m.CommuneId, 2, m)); 
        }

        var grouped = contributions.GroupBy(x => new { x.IndustryGroupId, x.RegionId, x.RegionLevel });
        var now = DateTime.UtcNow;

        foreach (var g in grouped)
        {
            var mList = g.Select(x => x.Metric).ToList();
            result.Add(new IndustryGroupAggregate
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
            });
        }
        return result;
    }

    private static List<VillageTypeAggregate> BuildVillageTypeAggregates(List<VillagePeriodicMetric> metrics, string period, byte periodType)
    {
        var result = new List<VillageTypeAggregate>();
        var contributions = new List<(int VillageTypeId, int RegionId, byte RegionLevel, VillagePeriodicMetric Metric)>();
        
        foreach (var m in metrics)
        {
            if (m.CraftVillage?.VillageTypeId == null) continue;
            var vtId = m.CraftVillage.VillageTypeId.Value;
            contributions.Add((vtId, 0, 0, m)); 
            if (m.ProvinceId > 0) contributions.Add((vtId, m.ProvinceId, 1, m)); 
            if (m.CommuneId > 0) contributions.Add((vtId, m.CommuneId, 2, m)); 
        }

        var grouped = contributions.GroupBy(x => new { x.VillageTypeId, x.RegionId, x.RegionLevel });
        var now = DateTime.UtcNow;

        foreach (var g in grouped)
        {
            var mList = g.Select(x => x.Metric).ToList();
            result.Add(new VillageTypeAggregate
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
            });
        }
        return result;
    }
}