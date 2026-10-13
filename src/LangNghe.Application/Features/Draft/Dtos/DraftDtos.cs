using LangNghe.Application.Common;
using System;

namespace LangNghe.Application.Features.Draft.Dtos;

public record DraftDto(
    long Id,
    int? CraftVillageId,
    string? DraftVillageCode,
    string DraftData,
    int CreatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? AutoSavedAt,
    byte DraftStatus,
    DateTime? SubmittedAt
);

public record SaveDraftDto(
    int? CraftVillageId,
    string? DraftVillageCode,
    string DraftData
);

public record DraftFilterDto : PageRequest
{
    public int? CraftVillageId { get; set; }
}
