namespace LangNghe.Application.Features.Village.Commands;

using LangNghe.Application.Common;
using LangNghe.Application.Features.Geocoding.Dtos;
using LangNghe.Application.Features.Geocoding.Interfaces;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GeocodeVillageCommand(int VillageId) : IRequest<ApiResponse<GeocodeResultDto>>;

public class GeocodeVillageCommandHandler : IRequestHandler<GeocodeVillageCommand, ApiResponse<GeocodeResultDto>>
{
    private readonly ILangNgheDbContext _context;
    private readonly IGeocodingService _geocodingService;
    private readonly ICurrentUserService _currentUser;

    public GeocodeVillageCommandHandler(
        ILangNgheDbContext context,
        IGeocodingService geocodingService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _geocodingService = geocodingService;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<GeocodeResultDto>> Handle(
        GeocodeVillageCommand request,
        CancellationToken cancellationToken)
    {
        var village = await _context.CraftVillages
            .AsTracking()
            .Include(x => x.Region)
            .ThenInclude(x => x!.Parent)
            .FirstOrDefaultAsync(x => x.Id == request.VillageId, cancellationToken);

        if (village == null)
            throw new NotFoundException("CraftVillage", request.VillageId);

        var region = village.Region;
        var communeName = region?.Parent != null ? region.Name : null;
        var provinceName = region?.Parent?.Name ?? region?.Name;

        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(village.Address))
        {
            candidates.Add(string.Join(", ", new[]
            {
                village.Address,
                communeName,
                provinceName,
                "Việt Nam"
            }.Where(x => !string.IsNullOrWhiteSpace(x))));
        }

        if (!string.IsNullOrWhiteSpace(communeName))
            candidates.Add($"{communeName}, {provinceName}, Việt Nam");

        if (!string.IsNullOrWhiteSpace(provinceName))
            candidates.Add($"{provinceName}, Việt Nam");

        if (candidates.Count == 0)
            throw new BusinessException("Làng nghề chưa có địa chỉ để geocode.");

        GeocodeResultDto? result = null;
        string? usedAddress = null;

        foreach (var address in candidates)
        {
            result = await _geocodingService.GeocodeAsync(address, cancellationToken);
            if (result != null)
            {
                usedAddress = address;
                break;
            }

            if (address != candidates[^1])
                await Task.Delay(1100, cancellationToken);
        }

        if (result == null)
            throw new BusinessException("Không tìm thấy tọa độ cho địa chỉ này (đã thử cả cấp thôn, xã và tỉnh).");

        village.Latitude = result.Latitude;
        village.Longitude = result.Longitude;
        village.GeocodedAt = DateTime.UtcNow;
        village.GeocodeSource = result.Source ?? "Nominatim";
        village.UpdatedAt = DateTime.UtcNow;
        village.UpdatedBy = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<GeocodeResultDto>.Ok(
            result with { DisplayName = usedAddress },
            $"Đã geocode từ: {usedAddress}");
    }
}
