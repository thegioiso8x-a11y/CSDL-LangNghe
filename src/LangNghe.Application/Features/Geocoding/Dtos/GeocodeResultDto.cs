namespace LangNghe.Application.Features.Geocoding.Dtos;

/// <summary>
/// Kết quả geocoding.
/// </summary>
public record GeocodeResultDto(
    decimal Latitude,
    decimal Longitude,
    string? DisplayName,
    string? Source
);
