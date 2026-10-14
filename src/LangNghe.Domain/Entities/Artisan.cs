using System;
using LangNghe.Domain.Entities.Common;

namespace LangNghe.Domain.Entities;

/// <summary>
/// Nghệ nhân của làng nghề.
/// Map với bảng Dim_Artisans.
/// </summary>
public class Artisan : BaseEntity
{
    public int CraftVillageId { get; set; }
    public required string FullName { get; set; }
    public int? BirthYear { get; set; }
    public string? Gender { get; set; }
    public string? ArtisanLevel { get; set; }
    public int? RecognitionYear { get; set; }
    public string? Specialty { get; set; }
    public string? Phone { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public CraftVillage? CraftVillage { get; set; }
}
