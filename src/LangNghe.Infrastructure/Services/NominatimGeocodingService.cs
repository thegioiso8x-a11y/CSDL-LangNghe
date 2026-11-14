namespace LangNghe.Infrastructure.Services;

using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using LangNghe.Application.Features.Geocoding.Dtos;
using LangNghe.Application.Features.Geocoding.Interfaces;
using LangNghe.Domain.Exceptions;
using Microsoft.Extensions.Logging;

/// <summary>
/// Geocoding dùng Nominatim (OpenStreetMap). Miễn phí, không cần API key.
/// Rate limit: 1 request/giây. Đã cấu hình User-Agent theo yêu cầu Nominatim.
/// </summary>
public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NominatimGeocodingService> _logger;

    public NominatimGeocodingService(HttpClient httpClient, ILogger<NominatimGeocodingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GeocodeResultDto?> GeocodeAsync(string address, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new BusinessException("Địa chỉ không được để trống.");

        // Lần 1: thử với địa chỉ đầy đủ
        var result = await SearchAsync(address, ct);
        if (result != null)
            return result;

        // Fallback: nếu địa chỉ có >= 3 phần (VD: "Đại Áng, Thanh Trì, Hà Nội, Việt Nam")
        // thì thử lại với chuỗi rút gọn (2 phần tử cuối: "Hà Nội, Việt Nam").
        var parts = address
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

        if (parts.Length >= 3)
        {
            var fallbackAddress = string.Join(", ", parts.Skip(parts.Length - 2));
            _logger.LogInformation(
                "Geocode lần 1 thất bại, thử fallback: {Fallback} (gốc: {Address})",
                fallbackAddress, address);

            result = await SearchAsync(fallbackAddress, ct);
            if (result != null)
                return result;
        }

        return null;
    }

    /// <summary>
    /// Gọi Nominatim search cho 1 chuỗi địa chỉ. Trả null nếu không tìm thấy.
    /// Có áp dụng Rate Limit 1 request/giây trước mỗi lần gọi.
    /// </summary>
    private async Task<GeocodeResultDto?> SearchAsync(string address, CancellationToken ct)
    {
        // Tuân thủ Rate Limit của Nominatim: tối đa 1 request/giây.
        await Task.Delay(1100, ct);

        var url = $"https://nominatim.openstreetmap.org/search" +
                  $"?q={Uri.EscapeDataString(address)}" +
                  "&format=json" +
                  "&limit=1" +
                  "&countrycodes=vn" +
                  "&addressdetails=0";

        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Nominatim trả về {StatusCode} cho địa chỉ: {Address}", response.StatusCode, address);
                return null;
            }

            var results = await response.Content.ReadFromJsonAsync<List<NominatimResult>>(cancellationToken: ct);
            if (results == null || results.Count == 0)
                return null;

            var first = results[0];
            if (!decimal.TryParse(first.Lat, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) ||
                !decimal.TryParse(first.Lon, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
                return null;

            return new GeocodeResultDto(latitude, longitude, first.DisplayName, "Nominatim");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout khi geocode địa chỉ: {Address}", address);
            throw new BusinessException("Quá thời gian chờ phản hồi từ bản đồ (Timeout). Vui lòng thử lại.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi geocode địa chỉ: {Address}", address);
            throw new BusinessException($"Không thể geocode địa chỉ: {ex.Message}");
        }
    }

    public async Task<string?> ReverseGeocodeAsync(decimal latitude, decimal longitude, CancellationToken ct = default)
    {
        var url = $"https://nominatim.openstreetmap.org/reverse" +
                  $"?lat={latitude.ToString(CultureInfo.InvariantCulture)}" +
                  $"&lon={longitude.ToString(CultureInfo.InvariantCulture)}" +
                  "&format=json" +
                  "&accept-language=vi";

        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<NominatimReverseResult>(cancellationToken: ct);
            return result?.DisplayName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi reverse geocode cho ({Lat}, {Lng})", latitude, longitude);
            return null;
        }
    }

    private class NominatimResult
    {
        [JsonPropertyName("lat")] public string Lat { get; set; } = "";
        [JsonPropertyName("lon")] public string Lon { get; set; } = "";
        [JsonPropertyName("display_name")] public string? DisplayName { get; set; }
    }

    private class NominatimReverseResult
    {
        [JsonPropertyName("display_name")] public string? DisplayName { get; set; }
    }
}