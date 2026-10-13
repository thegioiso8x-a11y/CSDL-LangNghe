using LangNghe.Application.Common;

namespace LangNghe.Application.Features.VillageType.Dtos;

public record VillageTypeDto
{
    public int Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public bool IsTraditional { get; init; }
    public bool IsRestored { get; init; }
    public bool IsNew { get; init; }
    public int? SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record CreateVillageTypeDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public bool IsTraditional { get; init; }
    public bool IsRestored { get; init; }
    public bool IsNew { get; init; }
    public int? SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record UpdateVillageTypeDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public bool IsTraditional { get; init; }
    public bool IsRestored { get; init; }
    public bool IsNew { get; init; }
    public int? SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record VillageTypeFilterDto : PageRequest
{
    public string? Keyword { get; init; }
    public bool? IsActive { get; init; }
}
