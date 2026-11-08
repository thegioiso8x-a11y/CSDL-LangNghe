using LangNghe.Application.Common;
using LangNghe.Application.Features.Village.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Village.Queries;

/// <summary>
/// Thống kê tổng quan làng nghề cho các thẻ KPI trên trang quản lý.
/// Nhận cùng bộ tham số lọc như API lấy danh sách để số liệu thay đổi linh hoạt theo bộ lọc.
/// </summary>
public record GetVillageStatsQuery(VillageFilterDto Dto) : IRequest<ApiResponse<VillageStatsDto>>;

/// <summary>
/// DTO chứa các chỉ số thống kê làng nghề.
/// </summary>
public record VillageStatsDto(
    int TotalVillages,
    int ActiveVillages,
    int PendingVillages,
    int PublishedVillages
);

public class GetVillageStatsQueryHandler : IRequestHandler<GetVillageStatsQuery, ApiResponse<VillageStatsDto>>
{
    private readonly ILangNgheDbContext _context;

    public GetVillageStatsQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<VillageStatsDto>> Handle(GetVillageStatsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Dto;
        var query = _context.CraftVillages.AsNoTracking().AsQueryable();

        // Áp dụng cùng điều kiện lọc như GetVillageListQuery
        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            query = query.Where(v => v.Name.Contains(filter.Keyword) || v.VillageCode.Contains(filter.Keyword));
        }

        if (filter.ProvinceId.HasValue)
        {
            query = query.Where(v => v.Region != null && v.Region.ParentId == filter.ProvinceId);
        }

        if (filter.CommuneId.HasValue)
        {
            query = query.Where(v => v.RegionId == filter.CommuneId);
        }

        if (filter.VillageTypeId.HasValue)
        {
            query = query.Where(v => v.VillageTypeId == filter.VillageTypeId);
        }

        if (filter.IndustryGroupId.HasValue)
        {
            query = query.Where(v => v.IndustryGroupId == filter.IndustryGroupId);
        }

        if (filter.ClassificationId.HasValue)
        {
            query = query.Where(v => v.ClassificationId == filter.ClassificationId);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(v => v.Status == filter.Status);
        }

        if (filter.IsPublished.HasValue)
        {
            query = query.Where(v => v.IsPublished == filter.IsPublished);
        }

        var totalVillages = await query.CountAsync(cancellationToken);

        // Đang hoạt động: Status = 1 (Hoạt động) hoặc 3 (Đã công bố)
        var activeVillages = await query
            .CountAsync(v => v.Status == 1 || v.Status == 3, cancellationToken);

        // Bản nháp / Chờ duyệt: Status = 0 (Draft), 1 (PendingProvince), 2 (ApprovedProvince)
        var pendingVillages = await query
            .CountAsync(v => v.Status == 0 || v.Status == 1 || v.Status == 2, cancellationToken);

        // Đã công bố
        var publishedVillages = await query
            .CountAsync(v => v.IsPublished, cancellationToken);

        var dto = new VillageStatsDto(totalVillages, activeVillages, pendingVillages, publishedVillages);
        return ApiResponse<VillageStatsDto>.Ok(dto);
    }
}
