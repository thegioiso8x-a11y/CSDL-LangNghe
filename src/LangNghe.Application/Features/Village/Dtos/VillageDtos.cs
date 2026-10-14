using LangNghe.Application.Common;

namespace LangNghe.Application.Features.Village.Dtos;

public record VillageListDto(
    int Id,
    string VillageCode,
    string Name,
    string? ImageUrl,
    string? ProvinceName,
    string? CommuneName,
    string? VillageTypeName,
    string? ClassificationName,
    byte Status,
    bool IsPublished,
    DateTime? RecognitionDate,
    DateTime? UpdatedAt
);

public record VillageDetailDto(
    int Id,
    string VillageCode,
    string Name,
    int RegionId,
    string? ProvinceName,
    string? CommuneName,
    string? Address,
    string? ImageUrl,
    string? RecognitionDecision,
    DateTime? RecognitionDate,
    int? ClassificationId,
    string? ClassificationName,
    int? VillageTypeId,
    string? VillageTypeName,
    int? IndustryGroupId,
    string? IndustryGroupName,
    bool NeedRestoration,
    byte Status,
    bool IsPublished,
    DateTime? PublishedAt,
    string RowVersion,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateVillageDto(
    string VillageCode,
    string Name,
    int RegionId,
    string? Address,
    string? ImageUrl,
    string? RecognitionDecision,
    DateTime? RecognitionDate,
    int? ClassificationId,
    int? VillageTypeId,
    int? IndustryGroupId,
    bool NeedRestoration
);

public record UpdateVillageDto(
    string VillageCode,
    string Name,
    int RegionId,
    string? Address,
    string? ImageUrl,
    string? RecognitionDecision,
    DateTime? RecognitionDate,
    int? ClassificationId,
    int? VillageTypeId,
    int? IndustryGroupId,
    bool NeedRestoration,
    byte Status,
    string RowVersionBase64
);

public record VillageFilterDto : PageRequest
{
    public string? Keyword { get; set; }
    public int? ProvinceId { get; set; }
    public int? CommuneId { get; set; }
    public int? VillageTypeId { get; set; }
    public int? IndustryGroupId { get; set; }
    public int? ClassificationId { get; set; }
    public byte? Status { get; set; }
    public bool? IsPublished { get; set; }
}
