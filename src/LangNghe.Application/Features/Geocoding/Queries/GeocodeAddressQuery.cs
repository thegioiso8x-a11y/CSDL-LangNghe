namespace LangNghe.Application.Features.Geocoding.Queries;

using LangNghe.Application.Common;
using LangNghe.Application.Features.Geocoding.Dtos;
using LangNghe.Application.Features.Geocoding.Interfaces;
using LangNghe.Domain.Exceptions;
using MediatR;

/// <summary>
/// Geocode một địa chỉ chưa gắn với làng nghề đã tồn tại.
/// </summary>
public record GeocodeAddressQuery(string Address) : IRequest<ApiResponse<GeocodeResultDto>>;

public class GeocodeAddressQueryHandler
    : IRequestHandler<GeocodeAddressQuery, ApiResponse<GeocodeResultDto>>
{
    private readonly IGeocodingService _geocodingService;

    public GeocodeAddressQueryHandler(IGeocodingService geocodingService)
    {
        _geocodingService = geocodingService;
    }

    public async Task<ApiResponse<GeocodeResultDto>> Handle(
        GeocodeAddressQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _geocodingService.GeocodeAsync(request.Address, cancellationToken);

        if (result == null)
            throw new NotFoundException($"Không tìm thấy tọa độ cho địa chỉ: {request.Address}", request.Address);

        return ApiResponse<GeocodeResultDto>.Ok(result);
    }
}
