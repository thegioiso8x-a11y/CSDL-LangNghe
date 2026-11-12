using System.Linq;
using LangNghe.Domain.Entities;
using LangNghe.Domain.Interfaces;

namespace LangNghe.Application.Common;

/// <summary>
/// Extension method áp dụng bộ lọc Data Scope (phân quyền theo địa bàn) một cách tường minh.
/// Thay thế cho Global Query Filter (vốn bị cache trong OnModelCreating).
/// Quy ước:
///  - DataScope == 1 (TW) hoặc null: không lọc (thấy tất cả).
///  - DataScope == 2 (Tỉnh): chỉ dữ liệu thuộc các Xã có Tỉnh mẹ = RegionId của user.
///  - DataScope == 3 (Xã): chỉ dữ liệu thuộc đúng Xã = RegionId của user.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>Áp dụng Data Scope cho danh sách Làng nghề.</summary>
    public static IQueryable<CraftVillage> ApplyDataScopeFilter(
        this IQueryable<CraftVillage> query,
        ICurrentUserService currentUserService)
    {
        var dataScope = currentUserService.DataScope;
        var userRegionId = currentUserService.RegionId;

        if (dataScope == null || dataScope == 1)
            return query;

        if (dataScope == 2)
            return query.Where(x => x.Region != null && x.Region.ParentId == userRegionId);

        if (dataScope == 3)
            return query.Where(x => x.RegionId == userRegionId);

        return query;
    }

    /// <summary>Áp dụng Data Scope cho danh sách Sản phẩm (lọc qua CraftVillage).</summary>
    public static IQueryable<Product> ApplyDataScopeFilter(
        this IQueryable<Product> query,
        ICurrentUserService currentUserService)
    {
        var dataScope = currentUserService.DataScope;
        var userRegionId = currentUserService.RegionId;

        if (dataScope == null || dataScope == 1)
            return query;

        if (dataScope == 2)
            return query.Where(x => x.CraftVillage != null
                && x.CraftVillage.Region != null
                && x.CraftVillage.Region.ParentId == userRegionId);

        if (dataScope == 3)
            return query.Where(x => x.CraftVillage != null && x.CraftVillage.RegionId == userRegionId);

        return query;
    }

    /// <summary>Áp dụng Data Scope cho danh sách Nghệ nhân (lọc qua CraftVillage).</summary>
    public static IQueryable<Artisan> ApplyDataScopeFilter(
        this IQueryable<Artisan> query,
        ICurrentUserService currentUserService)
    {
        var dataScope = currentUserService.DataScope;
        var userRegionId = currentUserService.RegionId;

        if (dataScope == null || dataScope == 1)
            return query;

        if (dataScope == 2)
            return query.Where(x => x.CraftVillage != null
                && x.CraftVillage.Region != null
                && x.CraftVillage.Region.ParentId == userRegionId);

        if (dataScope == 3)
            return query.Where(x => x.CraftVillage != null && x.CraftVillage.RegionId == userRegionId);

        return query;
    }

    /// <summary>
    /// Áp dụng Data Scope cho danh sách Chỉ tiêu định kỳ.
    /// VillagePeriodicMetric có sẵn ProvinceId/CommuneId trực tiếp nên lọc trực tiếp (không cần join).
    /// </summary>
    public static IQueryable<VillagePeriodicMetric> ApplyDataScopeFilter(
        this IQueryable<VillagePeriodicMetric> query,
        ICurrentUserService currentUserService)
    {
        var dataScope = currentUserService.DataScope;
        var userRegionId = currentUserService.RegionId;

        if (dataScope == null || dataScope == 1)
            return query;

        if (dataScope == 2)
            return query.Where(x => x.ProvinceId == userRegionId);

        if (dataScope == 3)
            return query.Where(x => x.CommuneId == userRegionId);

        return query;
    }
}
