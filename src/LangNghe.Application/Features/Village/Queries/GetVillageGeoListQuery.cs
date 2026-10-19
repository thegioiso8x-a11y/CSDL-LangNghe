using LangNghe.Application.Common;
using LangNghe.Application.Features.Village.Dtos;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Village.Queries;

/// <summary>
/// Truy vấn danh sách làng nghề đã có tọa độ phục vụ hiển thị bản đồ.
/// </summary>
public record GetVillageGeoListQuery(
    int? ProvinceId = null,
    int? VillageTypeId = null,
    string? Keyword = null) : IRequest<ApiResponse<List<VillageGeoDto>>>;

/// <summary>
/// Xử lý truy vấn danh sách tọa độ làng nghề.
/// </summary>
public class GetVillageGeoListQueryHandler
    : IRequestHandler<GetVillageGeoListQuery, ApiResponse<List<VillageGeoDto>>>
{
    private readonly ILangNgheDbContext _context;

    public GetVillageGeoListQueryHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy danh sách làng nghề đã có tọa độ theo điều kiện lọc.
    /// </summary>
    public async Task<ApiResponse<List<VillageGeoDto>>> Handle(
        GetVillageGeoListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.CraftVillages
            .AsNoTracking()
            .Where(v => v.Latitude != null && v.Longitude != null);

        if (request.ProvinceId.HasValue)
        {
            query = query.Where(v => v.Region != null && v.Region.ParentId == request.ProvinceId);
        }

        if (request.VillageTypeId.HasValue)
        {
            query = query.Where(v => v.VillageTypeId == request.VillageTypeId);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query = query.Where(v =>
                v.Name.Contains(request.Keyword) || v.VillageCode.Contains(request.Keyword));
        }

        var items = await query
            .OrderBy(v => v.Name)
            .Select(v => new VillageGeoDto(
                v.Id,
                v.VillageCode,
                v.Name,
                v.ImageUrl,
                v.Latitude.GetValueOrDefault(),
                v.Longitude.GetValueOrDefault(),
                v.Region != null && v.Region.Parent != null ? v.Region.Parent.Name : null,
                v.Region != null ? v.Region.Name : null,
                v.VillageType != null ? v.VillageType.Name : null,
                v.Classification != null ? v.Classification.Name : null,
                v.IsPublished))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<VillageGeoDto>>.Ok(items);
    }
}
