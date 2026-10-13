using LangNghe.Application.Common;

namespace LangNghe.Application.Features.Classification.Dtos;

public record ClassificationDto
{
    public int Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public int? SortOrder { get; init; }
}

public record CreateClassificationDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public int? SortOrder { get; init; }
}

public record UpdateClassificationDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public int? SortOrder { get; init; }
}

public record ClassificationFilterDto : PageRequest
{
    public string? Keyword { get; init; }
    
}
