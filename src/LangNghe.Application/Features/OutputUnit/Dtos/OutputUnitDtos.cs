using LangNghe.Application.Common;

namespace LangNghe.Application.Features.OutputUnit.Dtos;

/// <summary>
/// Dto cho Đơn vị tính sản lượng.
/// </summary>
public record OutputUnitDto
{
    public int Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public int? SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record CreateOutputUnitDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public int? SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record UpdateOutputUnitDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public int? SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record OutputUnitFilterDto : PageRequest
{
    public string? Keyword { get; init; }
    public bool? IsActive { get; init; }
}
