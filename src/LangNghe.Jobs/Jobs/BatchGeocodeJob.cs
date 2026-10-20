namespace LangNghe.Jobs.Jobs;

using LangNghe.Application.Features.Geocoding.Interfaces;
using LangNghe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Geocode tối đa 50 làng nghề chưa có đủ tọa độ trong mỗi lần chạy.
/// </summary>
public class BatchGeocodeJob
{
    private readonly ILangNgheDbContext _context;
    private readonly IGeocodingService _geocodingService;

    public BatchGeocodeJob(
        ILangNgheDbContext context,
        IGeocodingService geocodingService)
    {
        _context = context;
        _geocodingService = geocodingService;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var villages = await _context.CraftVillages
            .Where(village => village.Latitude == null || village.Longitude == null)
            .Include(village => village.Region)
            .ThenInclude(region => region!.Parent)
            .Take(50)
            .ToListAsync(cancellationToken);

        for (var index = 0; index < villages.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var village = villages[index];
            var address = string.Join(", ", new[]
            {
                village.Address,
                village.Region?.Name,
                village.Region?.Parent?.Name
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

            var result = await _geocodingService.GeocodeAsync(address, cancellationToken);
            if (result != null)
            {
                village.Latitude = result.Latitude;
                village.Longitude = result.Longitude;
                village.GeocodedAt = DateTime.UtcNow;
                village.GeocodeSource = "BatchHangfire";
            }

            if (index < villages.Count - 1)
                await Task.Delay(1100, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
