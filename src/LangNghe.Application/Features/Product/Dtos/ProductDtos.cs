using System;
using LangNghe.Application.Common;

namespace LangNghe.Application.Features.Product.Dtos;

public record ProductDto(
    int Id,
    int CraftVillageId,
    string? VillageName,
    string? ProductCode,
    string Name,
    string? Description,
    string? Materials,
    string? ImageUrl,
    bool HasOCOP,
    byte? OcopStar,
    int? OcopEvaluatedYear,
    string? MainMarket,
    int? SortOrder,
    bool IsActive,
    DateTime? UpdatedAt
);

public record CreateProductDto(
    int CraftVillageId,
    string? ProductCode,
    string Name,
    string? Description,
    string? Materials,
    string? ImageUrl,
    bool HasOCOP,
    byte? OcopStar,
    int? OcopEvaluatedYear,
    string? MainMarket,
    int? SortOrder
);

public record UpdateProductDto(
    int CraftVillageId,
    string? ProductCode,
    string Name,
    string? Description,
    string? Materials,
    string? ImageUrl,
    bool HasOCOP,
    byte? OcopStar,
    int? OcopEvaluatedYear,
    string? MainMarket,
    int? SortOrder,
    bool IsActive
);

public record ProductFilterDto : PageRequest
{
    public string? Keyword { get; init; }
    public int? CraftVillageId { get; init; }
    public bool? HasOCOP { get; init; }
    public byte? OcopStar { get; init; }
    public bool? IsActive { get; init; }
}
