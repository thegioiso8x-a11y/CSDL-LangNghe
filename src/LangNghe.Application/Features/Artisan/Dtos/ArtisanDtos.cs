using System;
using LangNghe.Application.Common;

namespace LangNghe.Application.Features.Artisan.Dtos;

public record ArtisanDto(
    int Id,
    int CraftVillageId,
    string? VillageName,
    string FullName,
    int? BirthYear,
    string? Gender,
    string? ArtisanLevel,
    int? RecognitionYear,
    string? Specialty,
    string? Phone,
    string? ImageUrl,
    bool IsActive,
    DateTime? UpdatedAt
);

public record CreateArtisanDto(
    int CraftVillageId,
    string FullName,
    int? BirthYear,
    string? Gender,
    string? ArtisanLevel,
    int? RecognitionYear,
    string? Specialty,
    string? Phone,
    string? ImageUrl
);

public record UpdateArtisanDto(
    int CraftVillageId,
    string FullName,
    int? BirthYear,
    string? Gender,
    string? ArtisanLevel,
    int? RecognitionYear,
    string? Specialty,
    string? Phone,
    string? ImageUrl,
    bool IsActive
);

public record ArtisanFilterDto : PageRequest
{
    public string? Keyword { get; init; }
    public int? CraftVillageId { get; init; }
    public string? ArtisanLevel { get; init; }
    public bool? IsActive { get; init; }
}
