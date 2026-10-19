namespace LangNghe.Application.Features.Geocoding.Interfaces;

using LangNghe.Application.Features.Geocoding.Dtos;

/// <summary>
/// Service geocoding — chuyển địa chỉ text thành tọa độ Lat/Lng.
/// </summary>
public interface IGeocodingService
{
    /// <summary>
    /// Geocode 1 địa chỉ. Trả null nếu không tìm thấy.
    /// </summary>
    Task<GeocodeResultDto?> GeocodeAsync(string address, CancellationToken ct = default);

    /// <summary>
    /// Geocode ngược — từ tọa độ trả về địa chỉ gần đúng.
    /// </summary>
    Task<string?> ReverseGeocodeAsync(decimal latitude, decimal longitude, CancellationToken ct = default);
}
