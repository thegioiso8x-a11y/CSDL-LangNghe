using LangNghe.Application.Common;

namespace LangNghe.Application.Features.ExportCategory.Dtos;

/// <summary>
/// Dto cho Nhóm mặt hàng xuất khẩu.
/// </summary>
public record ExportCategoryDto
{
    public int Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public int? SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record CreateExportCategoryDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public int? SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record UpdateExportCategoryDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public int? SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record ExportCategoryFilterDto : PageRequest
{
    public string? Keyword { get; init; }
    public bool? IsActive { get; init; }
}
