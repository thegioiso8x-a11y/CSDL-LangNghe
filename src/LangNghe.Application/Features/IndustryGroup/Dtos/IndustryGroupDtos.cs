using LangNghe.Application.Common;

namespace LangNghe.Application.Features.IndustryGroup.Dtos;

public record IndustryGroupDto
{
    public int Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public int? SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record CreateIndustryGroupDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public int? SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record UpdateIndustryGroupDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public int? SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record IndustryGroupFilterDto : PageRequest
{
    public string? Keyword { get; init; }
    public bool? IsActive { get; init; }
}
